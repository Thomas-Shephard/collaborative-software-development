using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Dapper;
using Jahoot.WebApi.Extensions;
using Jahoot.Core.Models;
using Jahoot.WebApi.Authorization;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

namespace Jahoot.WebApi;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddControllers();

        string? dbHost = builder.Configuration["DB_HOST"];
        string? dbPort = builder.Configuration["DB_PORT"];
        string? dbName = builder.Configuration["DB_NAME"];
        string? dbUser = builder.Configuration["DB_USER"];
        string? dbPassword = builder.Configuration["DB_PASSWORD"];

        if (string.IsNullOrEmpty(dbHost) || string.IsNullOrEmpty(dbPort) || string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword))
        {
            throw new InvalidOperationException("DB configuration (DB_HOST, DB_PORT, DB_NAME, DB_USER, or DB_PASSWORD) is not configured.");
        }

        string connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword}";

        builder.Services.AddScoped<IDbConnection>(_ => new MySqlConnection(connectionString));

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        
        string? smtpHost = builder.Configuration["SMTP_HOST"];
        string? smtpPort = builder.Configuration["SMTP_PORT"];
        string? smtpUser = builder.Configuration["SMTP_USER"];
        string? smtpPassword = builder.Configuration["SMTP_PASSWORD"];
        string? smtpFromEmail = builder.Configuration["SMTP_FROM_EMAIL"];
        string? smtpFromName = builder.Configuration["SMTP_FROM_NAME"];
        bool smtpEnableSsl = bool.TryParse(builder.Configuration["SMTP_ENABLE_SSL"], out bool ssl) && ssl;

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) || string.IsNullOrEmpty(smtpFromEmail) || string.IsNullOrEmpty(smtpFromName))
        {
             // If SMTP is not fully configured, we fall back to DummyEmailService for safety, 
             // or we could throw. Given this is a "collaborative" app, maybe fail fast is better?
             // But to avoid breaking existing setups without env vars, I'll throw if partial, but check existence.
             // Actually, the user said "idk what we should do...".
             // Let's throw to enforce config.
             throw new InvalidOperationException("SMTP configuration (SMTP_HOST, SMTP_PORT, SMTP_FROM_EMAIL, SMTP_FROM_NAME) is not configured.");
        }

        EmailSettings emailSettings = new()
        {
            Host = smtpHost,
            Port = int.Parse(smtpPort),
            User = smtpUser ?? string.Empty,
            Password = smtpPassword ?? string.Empty,
            FromEmail = smtpFromEmail,
            FromName = smtpFromName,
            EnableSsl = smtpEnableSsl
        };
        builder.Services.AddSingleton(emailSettings);
        builder.Services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();
        builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

        JwtSettings jwtSettings = new()
        {
            Secret = builder.Configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured."),
            Issuer = builder.Configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT_ISSUER is not configured."),
            Audience = builder.Configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT_AUDIENCE is not configured.")
        };
        builder.Services.AddSingleton(jwtSettings);

        LoginAttemptSettings loginAttemptSettings = builder.Services.AddAndConfigure<LoginAttemptSettings>(builder.Configuration, "LoginAttemptSettings");
        TokenDenySettings tokenDenySettings = builder.Services.AddAndConfigure<TokenDenySettings>(builder.Configuration, "TokenDenySettings");

        builder.Services.AddSingleton<ITokenDenyService>(_ => new TokenDenyService(tokenDenySettings, TimeProvider.System));
        builder.Services.AddSingleton<ILoginAttemptService>(_ => new LoginAttemptService(loginAttemptSettings, TimeProvider.System));
        builder.Services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();

        builder.Services.AddAuthorizationBuilder()
               .AddPolicy(nameof(Role.Admin), policy => policy.Requirements.Add(new RoleRequirement(Role.Admin)))
               .AddPolicy(nameof(Role.Lecturer), policy => policy.Requirements.Add(new RoleRequirement(Role.Lecturer)))
               .AddPolicy(nameof(Role.Student), policy => policy.Requirements.Add(new RoleRequirement(Role.Student)));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    ITokenDenyService denylist = context.HttpContext.RequestServices.GetRequiredService<ITokenDenyService>();
                    string? jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    if (jti != null && await denylist.IsDeniedAsync(jti))
                    {
                        context.Fail("Token is denied.");
                    }
                }
            };
        });

        WebApplication app = builder.Build();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
