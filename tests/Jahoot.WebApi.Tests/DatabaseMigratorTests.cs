using Dapper;
using Jahoot.WebApi.Tests.Repositories;
using MySqlConnector;

namespace Jahoot.WebApi.Tests;

public class DatabaseMigratorTests : RepositoryTestBase
{
    private string _dbName;
    private string _connectionString;

    [SetUp]
    public async Task SetUpTestDatabase()
    {
        _dbName = "migration_test_" + Guid.NewGuid().ToString("N");

        MySqlConnectionStringBuilder builder = new(TestDatabaseFactory.ConnectionString)
        {
            Database = _dbName
        };
        _connectionString = builder.ConnectionString;

        await using MySqlConnection setupConnection = new(TestDatabaseFactory.ConnectionString);
        await setupConnection.OpenAsync();
        await setupConnection.ExecuteAsync($"CREATE DATABASE {_dbName};");
    }

    [TearDown]
    public async Task TearDownTestDatabase()
    {
        await using MySqlConnection cleanupConnection = new(TestDatabaseFactory.ConnectionString);
        await cleanupConnection.OpenAsync();
        await cleanupConnection.ExecuteAsync($"DROP DATABASE IF EXISTS {_dbName};");
    }

    [Test]
    public async Task ApplyMigrations_ValidConnectionString_CreatesSchema()
    {
        DatabaseMigrator.ApplyMigrations(_connectionString);

        await using MySqlConnection connection = new(_connectionString);
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
        // Pre-create a table to cause the migration script (which creates 'User' table) to fail
        await using (MySqlConnection connection = new(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync("CREATE TABLE User (id INT PRIMARY KEY);");
        }

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => DatabaseMigrator.ApplyMigrations(_connectionString));
        Assert.That(ex.Message, Is.EqualTo("Database migration failed."));
    }
}
