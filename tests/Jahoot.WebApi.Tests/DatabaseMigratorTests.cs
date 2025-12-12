using Dapper;
using MySqlConnector;
using Testcontainers.MariaDb;

namespace Jahoot.WebApi.Tests;

[TestFixture]
[Category("Integration")]
public class DatabaseMigratorTests
{
    private MariaDbContainer _mariaDbContainer;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _mariaDbContainer = new MariaDbBuilder()
            .WithImage("mariadb:latest")
            .WithUsername("root")
            .WithPassword("root")
            .Build();
        await _mariaDbContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _mariaDbContainer.DisposeAsync();
    }

    [Test]
    public async Task ApplyMigrations_ValidConnectionString_CreatesSchema()
    {
        string connectionString = _mariaDbContainer.GetConnectionString();

        DatabaseMigrator.ApplyMigrations(connectionString);

        await using MySqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        IEnumerable<string> tables = await connection.QueryAsync<string>("SHOW TABLES;");
        List<string> tableList = tables.ToList();

        Assert.That(tableList, Does.Contain("User"));
        Assert.That(tableList, Does.Contain("Student"));
        Assert.That(tableList, Does.Contain("SchemaVersions").IgnoreCase); // DbUp's tracking table
    }

    [Test]
    public async Task ApplyMigrations_ScriptExecutionFails_ThrowsInvalidOperationException()
    {
        string dbName = "fail_test_" + Guid.NewGuid().ToString("N");
        MySqlConnectionStringBuilder builder = new(_mariaDbContainer.GetConnectionString())
        {
            Database = dbName
        };
        string connectionString = builder.ConnectionString;

        await using (MySqlConnection setupConnection = new(_mariaDbContainer.GetConnectionString()))
        {
            await setupConnection.OpenAsync();
            await setupConnection.ExecuteAsync($"CREATE DATABASE {dbName};");
            await setupConnection.ExecuteAsync($"USE {dbName}; CREATE TABLE User (id INT PRIMARY KEY);");
        }

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => DatabaseMigrator.ApplyMigrations(connectionString));
        Assert.That(ex.Message, Is.EqualTo("Database migration failed."));
    }
}
