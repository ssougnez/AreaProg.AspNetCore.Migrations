namespace AreaProg.AspNetCore.Migrations.Demo.Data.Entities;

/// <summary>
/// Tracks applied application migrations.
/// This table stores the version history of migrations that have been executed.
/// </summary>
public class MigrationHistory
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
}
