using AreaProg.AspNetCore.Migrations.Demo.Data;
using AreaProg.AspNetCore.Migrations.Demo.Data.Entities;
using AreaProg.AspNetCore.Migrations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AreaProg.AspNetCore.Migrations.Demo.Migrations;

/// <summary>
/// Application migration engine that stores version history in the database.
/// </summary>
/// <remarks>
/// This is an example implementation showing how to:
/// - Store migration versions in a database table
/// - Use lifecycle hooks for logging and custom logic
/// - Conditionally run migrations based on configuration
/// </remarks>
public class AppMigrationEngine : BaseMigrationEngine
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AppMigrationEngine> _logger;
    private readonly IConfiguration _configuration;

    public AppMigrationEngine(
        AppDbContext dbContext,
        ILogger<AppMigrationEngine> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Controls whether migrations should run.
    /// Can be disabled via configuration for specific environments.
    /// </summary>
    public override bool ShouldRun =>
        _configuration.GetValue("Migrations:Enabled", defaultValue: true);

    /// <summary>
    /// Retrieves all previously applied migration versions from the database.
    /// </summary>
    public override async Task<Version[]> GetAppliedVersionsAsync()
    {
        // Check if the MigrationHistory table exists before querying
        // This handles the first-run scenario where EF Core migrations haven't run yet
        var tableExists = await TableExistsAsync("MigrationHistory");
        if (!tableExists)
        {
            _logger.LogDebug("MigrationHistory table does not exist yet, returning empty version list");
            return Array.Empty<Version>();
        }

        var versions = await _dbContext.MigrationHistory
            .Select(m => m.Version)
            .ToListAsync();

        return versions
            .Select(v => Version.Parse(v))
            .ToArray();
    }

    private async Task<bool> TableExistsAsync(string tableName)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Records a migration version as applied in the database.
    /// </summary>
    public override async Task RegisterVersionAsync(Version version)
    {
        var exists = await _dbContext.MigrationHistory
            .AnyAsync(m => m.Version == version.ToString());

        if (!exists)
        {
            _dbContext.MigrationHistory.Add(new MigrationHistory
            {
                Version = version.ToString(),
                AppliedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Registered migration version {Version}", version);
        }
    }

    /// <summary>
    /// Called before any migrations run.
    /// Use this for initialization or validation.
    /// </summary>
    public override Task RunBeforeAsync()
    {
        _logger.LogInformation("Starting application migrations...");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after all migrations complete.
    /// Use this for cleanup or finalization.
    /// </summary>
    public override Task RunAfterAsync()
    {
        _logger.LogInformation("Application migrations completed successfully.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called before EF Core database migrations.
    /// Use this to capture data that needs to be preserved across schema changes.
    /// </summary>
    /// <param name="cache">Dictionary to store data for later use in migrations.</param>
    public override async Task RunBeforeDatabaseMigrationAsync(IDictionary<string, object> cache)
    {
        _logger.LogInformation("Preparing for database schema migrations...");

        // Example: Capture data before schema changes
        // This is useful when changing column types and you need to preserve/transform data
        // Check if table exists first to avoid error logs on initial setup
        if (await TableExistsAsync("Products"))
        {
            var productCount = await _dbContext.Products.CountAsync();
            cache["ProductCountBeforeMigration"] = productCount;
            _logger.LogInformation("Captured {Count} products before migration", productCount);
        }
        else
        {
            cache["ProductCountBeforeMigration"] = 0;
            _logger.LogDebug("Products table does not exist yet, skipping data capture");
        }
    }

    /// <summary>
    /// Called after EF Core database migrations complete.
    /// Use this for post-schema-change tasks.
    /// </summary>
    public override Task RunAfterDatabaseMigrationAsync()
    {
        _logger.LogInformation("Database schema migrations completed.");
        return Task.CompletedTask;
    }
}
