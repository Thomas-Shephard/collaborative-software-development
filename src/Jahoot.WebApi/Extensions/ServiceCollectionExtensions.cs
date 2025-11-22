using System.Diagnostics.CodeAnalysis;

namespace Jahoot.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static T AddAndConfigure<T>(this IServiceCollection services, IConfiguration configuration, string sectionName) where T : class
    {
        T settings = configuration.GetSection(sectionName).Get<T>() ?? throw new InvalidOperationException($"{sectionName} is not configured.");
        services.AddSingleton(settings);
        return settings;
    }
}
