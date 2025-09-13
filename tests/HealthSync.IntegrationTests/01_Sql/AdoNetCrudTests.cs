using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;
using HealthSync.IntegrationTests.Common;

namespace HealthSync.IntegrationTests.Sql;

public class AdoNetCrudTests
{
    [Fact]
    public async Task CreateDb_CreateTable_Insert_Select_And_Drop_Should_Work()
    {
        var db = TestSql.NewDbName();
        await TestSql.CreateDatabaseAsync(db);

        try
        {
            var cs = TestSql.BuildDbConnectionString(db);
            await using var conn = await TestSql.OpenWithRetryAsync(cs);

            // create table
            await using (var create = new SqlCommand(@"
                IF OBJECT_ID('dbo.Smoke','U') IS NULL
                CREATE TABLE dbo.Smoke(
                    Id INT NOT NULL PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL
                );", conn))
            {
                await create.ExecuteNonQueryAsync();
            }

            // insert
            await using (var insert = new SqlCommand("INSERT INTO dbo.Smoke (Id, Name) VALUES (1, N'OK');", conn))
            {
                var rows = await insert.ExecuteNonQueryAsync();
                rows.Should().Be(1);
            }

            // select
            await using (var select = new SqlCommand("SELECT COUNT(*) FROM dbo.Smoke;", conn))
            {
                var count = (int)(await select.ExecuteScalarAsync() ?? -1);
                count.Should().Be(1);
            }
        }
        finally
        {
            await TestSql.DropDatabaseAsync(db);
        }
    }
}
