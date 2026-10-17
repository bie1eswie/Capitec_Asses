using System;
using Xunit;

namespace Bams.Integration.Tests;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BAMS_TEST_POSTGRES")))
        {
            Skip = "Set BAMS_TEST_POSTGRES to a PostgreSQL connection string to run integration tests.";
        }
    }
}
