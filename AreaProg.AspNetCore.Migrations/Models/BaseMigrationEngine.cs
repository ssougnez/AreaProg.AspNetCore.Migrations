namespace AreaProg.AspNetCore.Migrations.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class for migration engine.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BaseMigrationEngine
    {
        /// <summary>
        /// Defines whether the migration must be applied or not.
        /// </summary>
        public virtual bool ShouldRun => true;

        /// <summary>
        /// Returns a list of all application migrations that already got applied.
        /// </summary>
        /// <returns>Application migrations that already got applied.</returns>
        public abstract Task<Version[]> GetAppliedVersionAsync();

        /// <summary>
        /// Returns the current version of the application.
        /// </summary>
        /// <returns>Current version of the application.</returns>
        public abstract Task<Version> GetCurrentVersionAsync();

        /// <summary>
        /// Method called to register the version in the external system.
        /// </summary>
        /// <returns></returns>
        public abstract Task RegisterVersionAsync(Version version);

        /// <summary>
        /// Method called after migrations are applied.
        /// </summary>
        /// <returns></returns>
        public virtual Task RunAfterAsync() => Task.CompletedTask;

        /// <summary>
        /// Method called before any migrations get applied.
        /// </summary>
        /// <returns></returns>
        public virtual Task RunBeforeAsync() => Task.CompletedTask;

        /// <summary>
        /// Method called just after the database has been migration by EF Core.
        /// </summary>
        /// <returns></returns>
        public virtual Task RunAfterDatabaseMigration() => Task.CompletedTask;
    }
}
