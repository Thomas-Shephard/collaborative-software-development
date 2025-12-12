using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Jahoot.WebApi.Extensions;

public static partial class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public T AddAndConfigure<T>(IConfiguration configuration, string sectionName) where T : class
        {
            T settings = configuration.GetSection(sectionName).Get<T>() ?? throw new InvalidOperationException($"{sectionName} is not configured.");
            services.AddSingleton(settings);
            return settings;
        }

        public T AddAndConfigureFromEnv<T>(IConfiguration configuration, string prefix) where T : class
        {
            T settings = Activator.CreateInstance<T>() ?? throw new InvalidOperationException($"Could not instantiate {typeof(T).Name}");

            foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanWrite))
            {
                string snakeCaseName = CamelToSnakeCase(prop.Name).ToUpper();
                string envKey = $"{prefix}_{snakeCaseName}";
                string? value = configuration[envKey];

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException($"Configuration value '{envKey}' is required.");
                }

                try
                {
                    object convertedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(settings, convertedValue);
                }
                catch
                {
                    throw new InvalidOperationException($"Failed to convert configuration value '{envKey}' to type {prop.PropertyType.Name}.");
                }
            }

            services.AddSingleton(settings);
            return settings;
        }
    }

    private static string CamelToSnakeCase(string input)
    {
        return string.IsNullOrEmpty(input)
            ? input
            : CamelToSnakeCaseRegex().Replace(input, "$1_$2");
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex CamelToSnakeCaseRegex();
}
