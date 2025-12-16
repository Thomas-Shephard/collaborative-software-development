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
using Jahoot.WebApi.Services.Background;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;

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
        builder.Services.AddOpenApi();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        DatabaseSettings dbSettings = builder.Services.AddAndConfigureFromEnv<DatabaseSettings>(builder.Configuration, "DB");

        DatabaseMigrator.ApplyMigrations(dbSettings.ConnectionString);

        builder.Services.AddScoped<IDbConnection>(_ =>
        {
            MySqlConnection connection = new(dbSettings.ConnectionString);
            connection.Open();
            return connection;
        });

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
        builder.Services.AddScoped<ITokenDenyRepository, TokenDenyRepository>();
        builder.Services.AddScoped<ILecturerRepository, LecturerRepository>();
        builder.Services.AddScoped<ITestRepository, TestRepository>();

        bool useMockEmailService = bool.TryParse(builder.Configuration["USE_MOCK_EMAIL_SERVICE"], out bool useMock) && useMock;

        if (useMockEmailService)
        {
            builder.Services.AddSingleton<IEmailService, MockEmailService>();
        }
        else
        {
            builder.Services.AddAndConfigureFromEnv<EmailSettings>(builder.Configuration, "SMTP");
            builder.Services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();
            builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
        }

        builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
        builder.Services.AddHostedService<EmailBackgroundService>();

        JwtSettings jwtSettings = builder.Services.AddAndConfigureFromEnv<JwtSettings>(builder.Configuration, "JWT");
        LoginAttemptSettings loginAttemptSettings = builder.Services.AddAndConfigure<LoginAttemptSettings>(builder.Configuration, "LoginAttemptSettings");
        TokenDenySettings tokenDenySettings = builder.Services.AddAndConfigure<TokenDenySettings>(builder.Configuration, "TokenDenySettings");

        builder.Services.AddSingleton<ITokenDenyService>(sp => new TokenDenyService(tokenDenySettings, TimeProvider.System, sp.GetRequiredService<IServiceScopeFactory>()));
        builder.Services.AddSingleton<ILoginAttemptService>(_ => new LoginAttemptService(loginAttemptSettings, TimeProvider.System));
        builder.Services.AddSingleton<ITokenService, TokenService>();
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

        app.MapOpenApi();
        app.MapScalarApiReference("/scalar");
        app.UseHttpsRedirection();
        app.UseForwardedHeaders();

        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
