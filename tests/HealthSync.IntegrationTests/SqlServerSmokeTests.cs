using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

public class SqlServerSmokeTests
{
    private static string BuildConnectionString()
    {
        // Zmiennie środowiskowe ustawi workflow GitHub Actions:
        // MSSQL_HOST, MSSQL_SA_USER, MSSQL_SA_PASSWORD
        var host = Environment.GetEnvironmentVariable("MSSQL_HOST") ?? "localhost,1433";
        var user = Environment.GetEnvironmentVariable("MSSQL_SA_USER") ?? "sa";
        var pass = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Your_strong_password_123";
        // Łączymy się do serwera (bez wskazania bazy — wystarczy do testu SELECT 1)
        return $"Server={host};User ID={user};Password={pass};TrustServerCertificate=True;";
    }

    [Fact]
    public async Task Can_Open_Connection_And_Select_1()
    {
        var cs = BuildConnectionString();

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("SELECT 1", conn);
        var result = (int) (await cmd.ExecuteScalarAsync() ?? -1);

        result.Should().Be(1);
    }
}
