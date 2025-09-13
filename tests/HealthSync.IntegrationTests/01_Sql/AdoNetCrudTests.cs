using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;
using HealthSync.IntegrationTests.Common;

namespace HealthSync.IntegrationTests.Sql;

public class AdoNetCrudTests
{
    [Fact]
    public async Task CreateTable_Insert_Select_Drop_Should_Work()
    {
        var db = TestSql.NewDb();
        await TestSql.CreateDbAsync(db);

        try
        {
            var cs = TestSql.Cs(db);
            await using var conn = await TestSql.OpenWithRetryAsync(cs);

            await using (var create = new SqlCommand(@"
                IF OBJECT_ID('dbo.Smoke','U') IS NULL
                CREATE TABLE dbo.Smoke(Id INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL);", conn))
            { await create.ExecuteNonQueryAsync(); }

            await using (var insert = new SqlCommand("INSERT INTO dbo.Smoke (Id, Name) VALUES (1, N'OK');", conn))
            { (await insert.ExecuteNonQueryAsync()).Should().Be(1); }

            await using (var select = new SqlCommand("SELECT COUNT(*) FROM dbo.Smoke;", conn))
            { ((int)(await select.ExecuteScalarAsync() ?? -1)).Should().Be(1); }
        }
        finally
        {
            await TestSql.DropDbAsync(db);
        }
    }
}
