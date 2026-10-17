using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace Bams.Infrastructure;

public sealed class BamsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<BamsDbContext>
{
    public BamsDbContext CreateDbContext(string[] args)
    {
        var apiSettingsDirectory = FindApiSettingsDirectory();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? ReadConnectionString(Path.Combine(apiSettingsDirectory, "appsettings.json"), "Postgres")
            ?? ReadConnectionString(Path.Combine(apiSettingsDirectory, $"appsettings.{environment}.json"), "Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");
        var options = new DbContextOptionsBuilder<BamsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BamsDbContext(options);
    }

    private static string? ReadConnectionString(string path, string name)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            || !connectionStrings.TryGetProperty(name, out var connectionString))
        {
            return null;
        }

        var value = connectionString.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string FindApiSettingsDirectory()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Bams.Api");
            if (File.Exists(Path.Combine(candidate, "Bams.Api.csproj")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("Could not locate src/Bams.Api configuration files from the current directory.");
    }
}
