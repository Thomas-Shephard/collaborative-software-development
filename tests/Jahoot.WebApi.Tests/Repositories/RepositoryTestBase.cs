using Dapper;
using Jahoot.Core.Models;
using MySqlConnector;
using Testcontainers.MariaDb;

namespace Jahoot.WebApi.Tests.Repositories;

[Category("Integration")]
public abstract class RepositoryTestBase
{
    private static MariaDbContainer _mariaDbContainer;
    protected MySqlConnection Connection;

    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        _mariaDbContainer = new MariaDbBuilder()
            .WithImage("mariadb:latest")
            .Build();
        await _mariaDbContainer.StartAsync();
        string script = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "database-init.sql"));
        await _mariaDbContainer.ExecScriptAsync(script);
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        await _mariaDbContainer.DisposeAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        Connection = new MySqlConnection(_mariaDbContainer.GetConnectionString());
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

    protected async Task<int> InsertUser(User user)
    {
        const string insertQuery = "INSERT INTO User (email, name, password_hash, created_at, updated_at) VALUES (@Email, @Name, @PasswordHash, @CreatedAt, @UpdatedAt); SELECT LAST_INSERT_ID();";
        return await Connection.QuerySingleAsync<int>(insertQuery, user);
    }

    protected async Task InsertLecturer(int userId, bool isAdmin)
    {
        await Connection.ExecuteAsync("INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, @IsAdmin)", new { UserId = userId, IsAdmin = isAdmin });
    }

    protected async Task InsertStudent(int userId)
    {
        await Connection.ExecuteAsync("INSERT INTO Student (user_id) VALUES (@UserId)", new { UserId = userId });
    }
}
