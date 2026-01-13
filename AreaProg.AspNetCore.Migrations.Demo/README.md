# AreaProg.AspNetCore.Migrations Demo

This demo application shows how to use the **AreaProg.AspNetCore.Migrations** NuGet package in a real ASP.NET Core application.

## Running the Demo

```bash
cd AreaProg.AspNetCore.Migrations.Demo
dotnet run
```

Then open http://localhost:5254/swagger to explore the API.

## Project Structure

```
AreaProg.AspNetCore.Migrations.Demo/
├── Data/
│   ├── Entities/
│   │   ├── Product.cs          # Sample entity
│   │   ├── Category.cs         # Sample entity
│   │   └── MigrationHistory.cs # Tracks applied migrations
│   ├── EFMigrations/           # EF Core migrations
│   └── AppDbContext.cs         # Database context
├── Migrations/
│   ├── AppMigrationEngine.cs   # Migration engine implementation
│   ├── V1_0_0_InitialSetup.cs  # First migration (demonstrates FirstTime)
│   ├── V1_1_0_AddCategories.cs # Second migration (demonstrates idempotence)
│   └── V1_2_0_AddProductMetrics.cs # Third migration (demonstrates Cache)
├── Program.cs                  # Application entry point
└── appsettings.json           # Configuration
```

## Key Concepts Demonstrated

### 1. Migration Engine Implementation

The `AppMigrationEngine` class shows how to implement `BaseMigrationEngine`:

```csharp
public class AppMigrationEngine : BaseMigrationEngine
{
    public override async Task<Version[]> GetAppliedVersionAsync()
    {
        // Retrieve applied versions from database
    }

    public override async Task RegisterVersionAsync(Version version)
    {
        // Store version in database
    }
}
```

### 2. FirstTime Property

The `V1_0_0_InitialSetup` migration demonstrates using `FirstTime` to guard one-time operations:

```csharp
public override async Task UpAsync()
{
    if (FirstTime)
    {
        // Seed data - only runs once
        _dbContext.Products.AddRange(products);
        await _dbContext.SaveChangesAsync();
    }
}
```

### 3. Idempotent Operations

The `V1_1_0_AddCategories` migration shows idempotent "upsert" patterns:

```csharp
var existing = await _dbContext.Categories
    .FirstOrDefaultAsync(c => c.Name == categoryData.Name);

if (existing == null)
{
    _dbContext.Categories.Add(new Category { ... });
}
```

### 4. Cache for Schema Changes

The `V1_2_0_AddProductMetrics` migration shows how to use data captured before EF migrations:

```csharp
// In MigrationEngine.RunBeforeDatabaseMigrationAsync:
cache["ProductCountBeforeMigration"] = await _dbContext.Products.CountAsync();

// In migration:
if (Cache.TryGetValue("ProductCountBeforeMigration", out var countObj))
{
    var productCountBefore = (int)countObj;
}
```

### 5. Lifecycle Hooks

The `AppMigrationEngine` demonstrates all lifecycle hooks:

- `RunBeforeAsync()` - Before any migrations
- `RunBeforeDatabaseMigrationAsync()` - Before EF Core migrations
- `RunAfterDatabaseMigrationAsync()` - After EF Core migrations
- `RunAfterAsync()` - After all migrations

### 6. Configuration

Migrations can be conditionally disabled via configuration:

```json
{
  "Migrations": {
    "Enabled": false
  }
}
```

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| GET /products | List all products with categories |
| POST /products | Create a new product |
| GET /categories | List all categories |
| GET /migrations | View migration history |

## Configuration in Program.cs

```csharp
// 1. Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=demo.db"));

// 2. Register migrations
builder.Services.AddApplicationMigrations<AppMigrationEngine>(options =>
{
    options.DbContext = typeof(AppDbContext);
});

// 3. Run migrations at startup
await app.UseMigrationsAsync();
```
