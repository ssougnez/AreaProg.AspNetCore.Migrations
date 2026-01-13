using AreaProg.AspNetCore.Migrations.Demo.Data;
using AreaProg.AspNetCore.Migrations.Models;
using Microsoft.Extensions.Logging;

namespace AreaProg.AspNetCore.Migrations.Demo.Migrations;

/// <summary>
/// Migration that demonstrates using the Cache property.
/// The Cache contains data captured in RunBeforeDatabaseMigrationAsync.
/// </summary>
public class V1_2_0_AddProductMetrics : BaseMigration
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<V1_2_0_AddProductMetrics> _logger;

    public V1_2_0_AddProductMetrics(AppDbContext dbContext, ILogger<V1_2_0_AddProductMetrics> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override Version Version => new(1, 2, 0);

    public override async Task UpAsync()
    {
        _logger.LogInformation("Running product metrics migration (FirstTime: {FirstTime})", FirstTime);

        // Access data that was captured in RunBeforeDatabaseMigrationAsync
        // This is useful when schema changes require data transformation
        if (Cache.TryGetValue("ProductCountBeforeMigration", out var countObj))
        {
            var productCountBefore = (int)countObj;
            _logger.LogInformation(
                "Product count before database migration: {Count}",
                productCountBefore);
        }

        if (FirstTime)
        {
            // Example: Log a summary of the current state
            var totalProducts = _dbContext.Products.Count();
            var totalCategories = _dbContext.Categories.Count();

            _logger.LogInformation(
                "Database state: {ProductCount} products, {CategoryCount} categories",
                totalProducts,
                totalCategories);
        }

        _logger.LogInformation("Product metrics migration completed");
        await Task.CompletedTask;
    }
}
