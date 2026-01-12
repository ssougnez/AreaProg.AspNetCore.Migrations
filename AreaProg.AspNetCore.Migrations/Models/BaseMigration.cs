namespace AreaProg.AspNetCore.Migrations.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class to define an application migration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BaseMigration
    {
        /// <summary>
        /// Application migration version.
        /// </summary>
        public abstract Version Version { get; }

        /// <summary>
        /// Define whether it's the first time the migration gets applied or not.
        /// </summary>
        public bool FirstTime { get; set; }

        /// <summary>
        /// Method called to apply the migration.
        /// </summary>
        public abstract Task UpAsync();
    }
}
