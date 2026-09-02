using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Recipe.Infrastructure;

public class TestDatabaseContextFactory : IDisposable
{
    private readonly List<DbConnection> dbConnections = new();

    private DbContextOptions<RecipeDbContext> CreateOptions(DbConnection connection)
    {
        return new DbContextOptionsBuilder<RecipeDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    public RecipeDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        dbConnections.Add(connection);

        var options = CreateOptions(connection);
        using (var init = new RecipeDbContext(options))
        {
            init.Database.EnsureCreated();
        }

        return new RecipeDbContext(options);
    }

    public void Dispose()
    {
        foreach (var c in dbConnections)
        {
            try { c.Dispose(); } catch { }
        }

        dbConnections.Clear();
    }
}
