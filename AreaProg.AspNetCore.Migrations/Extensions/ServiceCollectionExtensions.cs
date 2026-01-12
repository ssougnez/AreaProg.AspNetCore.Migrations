namespace AreaProg.AspNetCore.Migrations.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using AreaProg.AspNetCore.Migrations.Interfaces;
    using AreaProg.AspNetCore.Migrations.Services;
    using System;

    /// <summary>
    /// Migrations options
    /// </summary>
    public class ApplicationMigrationsOptions<T>
    {
        /// <summary>
        /// Entity Framework Core context used by the application. If defined, the migrations are applied.
        /// </summary>
        public Type DbContext { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure the application migration service
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddApplicationMigrations<T>(this IServiceCollection services, Action<ApplicationMigrationsOptions<T>> setupAction = null)
        {
            var options = new ApplicationMigrationsOptions<T>();

            setupAction?.Invoke(options);

            services.AddSingleton(options);

            services.AddSingleton<IApplicationMigrationEngine, ApplicationMigrationEngine<T>>();

            return services;
        }
    }
}
