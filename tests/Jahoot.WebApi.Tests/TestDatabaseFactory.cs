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

            _mariaDbContainer = new MariaDbBuilder()
                                .WithImage("mariadb:latest")
                                .WithUsername("root")
                                .WithPassword("root")
                                .Build();

            await _mariaDbContainer.StartAsync();

            ConnectionString = _mariaDbContainer.GetConnectionString();
            DatabaseMigrator.ApplyMigrations(ConnectionString);

            DefaultTypeMap.MatchNamesWithUnderscores = true;
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
