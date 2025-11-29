namespace Jahoot.WebApi.Settings;

public class DatabaseSettings
{
    public required string Host { get; init; }
    private static int Port => 3306;
    public required string Name { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }

    public string ConnectionString => $"Server={Host};Port={Port};Database={Name};User={User};Password={Password}";
}
