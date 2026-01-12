namespace AreaProg.AspNetCore.Migrations.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IApplicationMigrationEngine
    {
        /// <summary>
        /// Returns whether the migrations have been applied or not
        /// </summary>
        bool HasRun { get; }

        /// <summary>
        /// Apply the migrations
        /// </summary>
        void Run();
    }
}
