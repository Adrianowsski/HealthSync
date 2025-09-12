using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using HealthSync.Shared.Data;

namespace HealthSync.Shared.Tests.Utils;

public sealed class SqliteInMemoryContext : IDisposable
{
    private readonly SqliteConnection _conn;
    public AppDbContext Ctx { get; }

    public SqliteInMemoryContext()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conn)
            .EnableSensitiveDataLogging()
            .Options;

        Ctx = new AppDbContext(opts);
        Ctx.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Ctx.Dispose();
        _conn.Dispose();
    }
}
