using System;
using System.IO;
using System.Text.Json;

namespace Milese.Tests.Integration.Configuration;

public static class IntegrationTestSettingsLoader
{
    private const string SectionName = "IntegrationTests";
    private const string ProviderEnvVar = "MILESE_INTEGRATION_TESTS_PROVIDER";
    private const string ConnectionStringEnvVar = "MILESE_INTEGRATION_TESTS_CONNECTIONSTRING";

    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>
    /// Resolves integration test settings. Environment variables take priority over
    /// <c>appsettings.json</c>.
    /// </summary>
    public static IntegrationTestSettings Load()
    {
        var provider = Environment.GetEnvironmentVariable(ProviderEnvVar);
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);

        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(appSettingsPath))
        {
            using var stream = File.OpenRead(appSettingsPath);
            using var document = JsonDocument.Parse(stream, JsonOptions);

            if (document.RootElement.TryGetProperty(SectionName, out var section))
            {
                if (string.IsNullOrWhiteSpace(provider)
                    && section.TryGetProperty("Provider", out var providerElement))
                {
                    provider = providerElement.GetString();
                }

                if (string.IsNullOrWhiteSpace(connectionString)
                    && section.TryGetProperty("ConnectionString", out var connectionElement))
                {
                    connectionString = connectionElement.GetString();
                }
            }
        }

        var resolvedProvider = Enum.TryParse<TestDatabaseProvider>(provider, ignoreCase: true, out var parsed)
            ? parsed
            : TestDatabaseProvider.Sqlite;

        return new IntegrationTestSettings(resolvedProvider, connectionString);
    }
}
