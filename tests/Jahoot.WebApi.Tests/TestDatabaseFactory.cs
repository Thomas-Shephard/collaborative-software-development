using Dapper;
using Testcontainers.MariaDb;
using System.Threading;

namespace Jahoot.WebApi.Tests;

internal static class TestDatabaseFactory
{
    private static MariaDbContainer? _mariaDbContainer;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public static string ConnectionString { get; private set; } = string.Empty;

    public static async Task EnsureInitializedAsync()
    {
        if (Volatile.Read(ref _mariaDbContainer) is not null) return;

        await Lock.WaitAsync();
        try
        {
            if (_mariaDbContainer is not null) return;

            MariaDbContainer container = new MariaDbBuilder().WithImage("mariadb:latest").Build();

            await container.StartAsync();

            ConnectionString = container.GetConnectionString();

            string scriptPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "database-init.sql");
            if (File.Exists(scriptPath))
            {
                string script = await File.ReadAllTextAsync(scriptPath);
                await container.ExecScriptAsync(script);
            }
            else
            {
                throw new FileNotFoundException($"database-init.sql not found at {scriptPath}");
            }

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            _mariaDbContainer = container;
        }
        finally
        {
            Lock.Release();
        }
    }

    public static async Task DisposeAsync()
    {
        if (_mariaDbContainer != null)
        {
            await _mariaDbContainer.DisposeAsync();
            _mariaDbContainer = null;
        }
    }
}
