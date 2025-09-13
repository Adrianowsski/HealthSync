using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace HealthSync.IntegrationTests.Common;

public static class TestSql
{
    public static string Server => Environment.GetEnvironmentVariable("MSSQL_HOST") ?? "localhost,1433";
    public static string User   => Environment.GetEnvironmentVariable("MSSQL_SA_USER") ?? "sa";
    public static string Pass   => Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Your_strong_password_123";

    public static string MasterConnectionString =>
        $"Server={Server};User ID={User};Password={Pass};TrustServerCertificate=True;Initial Catalog=master;";

    public static string BuildDbConnectionString(string dbName) =>
        $"Server={Server};User ID={User};Password={Pass};TrustServerCertificate=True;Initial Catalog={dbName};";

    public static async Task<SqlConnection> OpenWithRetryAsync(string cs, int retries = 30, int delayMs = 2000)
    {
        Exception? last = null;
        for (int i = 0; i < retries; i++)
        {
            try
            {
                var conn = new SqlConnection(cs);
                await conn.OpenAsync();
                return conn;
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(delayMs);
            }
        }
        throw new TimeoutException("SQL Server not ready", last);
    }

    public static string NewDbName() => $"HealthSync_CI_{Guid.NewGuid():N}";

    public static async Task CreateDatabaseAsync(string dbName)
    {
        await using var master = await OpenWithRetryAsync(MasterConnectionString);
        await using var cmd = new SqlCommand($"IF DB_ID(@n) IS NULL CREATE DATABASE [{dbName}];", master);
        cmd.Parameters.AddWithValue("@n", dbName);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task DropDatabaseAsync(string dbName)
    {
        await using var master = await OpenWithRetryAsync(MasterConnectionString);
        var sql = $@"
IF DB_ID(@n) IS NOT NULL
BEGIN
  ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [{dbName}];
END";
        await using var cmd = new SqlCommand(sql, master);
        cmd.Parameters.AddWithValue("@n", dbName);
        await cmd.ExecuteNonQueryAsync();
    }
}
