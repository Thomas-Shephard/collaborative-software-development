using Dapper;
using MySqlConnector;

namespace Jahoot.WebApi.Tests.Repositories;

[Category("Integration")]
public abstract class RepositoryTestBase
{
    protected MySqlConnection Connection;

    [SetUp]
    public async Task Setup()
    {
        await TestDatabaseFactory.EnsureInitializedAsync();
        Connection = new MySqlConnection(TestDatabaseFactory.ConnectionString);
        await Connection.OpenAsync();

        await Connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 0;");
        IEnumerable<string> tables = await Connection.QueryAsync<string>("SHOW TABLES;");
        foreach (string table in tables)
        {
            await Connection.ExecuteAsync($"TRUNCATE TABLE {table};");
        }
        await Connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 1;");
    }

    [TearDown]
    public async Task TearDown()
    {
        await Connection.CloseAsync();
        await Connection.DisposeAsync();
    }
}
