using System;
using System.Threading;
using System.Threading.Tasks;
using Bams.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bams.Integration.Tests;

public sealed class PostgreSqlTestDatabase : IAsyncDisposable
{
    private readonly string adminConnectionString;

    public PostgreSqlTestDatabase(string adminConnectionString)
    {
        this.adminConnectionString = adminConnectionString;
        Schema = $"bams_it_{Guid.NewGuid():N}";
        var connectionString = new NpgsqlConnectionStringBuilder(adminConnectionString)
        {
            SearchPath = Schema
        };
        ConnectionString = connectionString.ConnectionString;
    }

    public string Schema { get; }

    public string ConnectionString { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE SCHEMA \"{Schema}\"";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public BamsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BamsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new BamsDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP SCHEMA IF EXISTS \"{Schema}\" CASCADE";
        await command.ExecuteNonQueryAsync();
    }
}
