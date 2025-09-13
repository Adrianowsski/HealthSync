using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace HealthSync.IntegrationTests.Common;

public static class TestSql
{
    public static string Server => Environment.GetEnvironmentVariable("MSSQL_HOST") ?? "localhost,1433";
    public static string User   => Environment.GetEnvironmentVariable("MSSQL_SA_USER") ?? "sa";
    public static string Pass   => Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Your_strong_password_123";

    public static string Master =>
        $"Server={Server};User ID={User};Password={Pass};TrustServerCertificate=True;Initial Catalog=master;";

    public static string Cs(string db) =>
        $"Server={Server};User ID={User};Password={Pass};TrustServerCertificate=True;Initial Catalog={db};";

    public static async Task<SqlConnection> OpenWithRetryAsync(string cs, int retries = 30, int delayMs = 2000)
    {
        Exception? last = null;
        for (int i = 0; i < retries; i++)
        {
            try
            {
                var c = new SqlConnection(cs);
                await c.OpenAsync();
                return c;
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(delayMs);
            }
        }
        throw new TimeoutException("SQL not ready", last);
    }

    public static string NewDb() => $"HealthSync_CI_{Guid.NewGuid():N}";

    public static async Task CreateDbAsync(string db)
    {
        await using var m = await OpenWithRetryAsync(Master);
        await using var cmd = new SqlCommand($"IF DB_ID(@n) IS NULL CREATE DATABASE [{db}];", m);
        cmd.Parameters.AddWithValue("@n", db);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task DropDbAsync(string db)
    {
        await using var m = await OpenWithRetryAsync(Master);
        var sql = $@"
IF DB_ID(@n) IS NOT NULL
BEGIN
  ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [{db}];
END";
        await using var cmd = new SqlCommand(sql, m);
        cmd.Parameters.AddWithValue("@n", db);
        await cmd.ExecuteNonQueryAsync();
    }
}
