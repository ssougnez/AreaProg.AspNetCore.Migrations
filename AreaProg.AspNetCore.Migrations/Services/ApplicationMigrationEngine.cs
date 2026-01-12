namespace AreaProg.AspNetCore.Migrations.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AreaProg.AspNetCore.Migrations.Extensions;
using AreaProg.AspNetCore.Migrations.Interfaces;
using AreaProg.AspNetCore.Migrations.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Application migrations engine
/// </summary>
public class ApplicationMigrationEngine<T> : IApplicationMigrationEngine
{
    private readonly ApplicationMigrationsOptions<T> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IApplicationMigrationEngine> _logger;

    private BaseMigration[] _applicationMigrations = null;
    private bool _hasRun = false;

    /// <summary>
    /// 
    /// </summary>
    public bool HasRun => _hasRun;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ApplicationMigrationEngine(
        IServiceProvider serviceProvider,
        ApplicationMigrationsOptions<T> options,
        ILogger<IApplicationMigrationEngine> logger
    )
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Run() => RunAsync().Wait();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task RunAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var engine = ActivatorUtilities.CreateInstance(scope.ServiceProvider, _options.GetType().GetGenericArguments()[0].UnderlyingSystemType) as BaseMigrationEngine;

        if (engine is null)
        {
            _logger.LogError("No migration engine defined");

            throw new ArgumentException();
        }
        else
        {
            if (engine.ShouldRun)
            {
                PopulateApplicationMigrations(scope, engine);

                await engine.RunBeforeAsync();

                await ApplyMigrationsAsync(engine, scope);

                await engine.RunAfterAsync();
            }
            else
            {
                _logger.LogDebug("Application migrations are configured not to run");
            }

            _hasRun = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    private async Task ApplyMigrationsAsync(BaseMigrationEngine engine, IServiceScope scope)
    {
        var applied = (await engine.GetAppliedVersionAsync()).OrderBy(v => v);
        var target = _applicationMigrations.LastOrDefault()?.Version ?? new Version(0, 0, 0);
        var current = applied.LastOrDefault() ?? new Version(0, 0, 0);

        if (current <= target)
        {
            var dbContext = _options.DbContext != null ? scope.ServiceProvider.GetService(_options.DbContext) as DbContext : null;

            if (dbContext != null)
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();

                _logger.LogInformation("Applying Entity Framework Core migrations...");

                await strategy.ExecuteAsync(async () =>
                {
                    dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(15));

                    await using var transaction = await dbContext.Database.BeginTransactionAsync();

                    await dbContext.Database.MigrateAsync();

                    await transaction.CommitAsync();
                });

                _logger.LogInformation("Entity Framework Core migrations applied");

                await engine.RunAfterDatabaseMigration();
            }

            var migrations = _applicationMigrations
                .Where(m => m.Version >= current && m.Version <= target)
                .OrderBy(m => m.Version);

            foreach (var migration in migrations)
            {
                _logger.LogInformation("Applying version {Version}", migration.Version);

                migration.FirstTime = !applied.Any(v => v == migration.Version);

                if (dbContext != null)
                {
                    using var transaction = await dbContext.Database.BeginTransactionAsync();

                    await migration.UpAsync();

                    await transaction.CommitAsync();
                }
                else
                {
                    await migration.UpAsync();
                }

                _logger.LogInformation("Version {Version} applied", migration.Version);

                if (migration.Version != current)
                {
                    _logger.LogInformation("Registering version {Version}...", migration.Version);

                    await engine.RegisterVersionAsync(migration.Version);

                    _logger.LogInformation("Version {Version} registered", migration.Version);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    private bool IsInheritingFrom(Type type, Type baseType)
    {
        while (type.BaseType != null)
        {
            if (type.BaseType == baseType)
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="engine"></param>
    private void PopulateApplicationMigrations(IServiceScope scope, BaseMigrationEngine engine)
    {
        _applicationMigrations = engine
            .GetType()
            .Assembly
            .GetTypes()
            .Where(t => IsInheritingFrom(t, typeof(BaseMigration)) && t.IsAbstract == false)
            .Select(t => ActivatorUtilities.CreateInstance(scope.ServiceProvider, t) as BaseMigration)
            .OrderBy(t => t.Version)
            .ToArray();
    }
}