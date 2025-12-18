    using Jahoot.Display.Services;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using System.Windows;
    using Microsoft.Extensions.Configuration;
    using System.IO;
    using System.Reflection;

    namespace Jahoot.Display;

    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        private IConfigurationRoot? _configuration;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureAppConfiguration(serviceCollection);
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureAppConfiguration(IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream("appsettings.json");

            if (stream is null)
            {
                throw new InvalidOperationException("Embedded appsettings.json not found.");
            }

            _configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            services.AddSingleton<IConfiguration>(_configuration);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISecureStorageService, SecureStorageService>();

            string baseAddress = _configuration?.GetValue<string>("BaseAddress")
                                 ?? throw new InvalidOperationException("BaseAddress is missing from configuration.");

            services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            });
            services.AddSingleton<IHttpService, HttpService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ISubjectService, SubjectService>();
            services.AddTransient<ILecturerService, LecturerService>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LecturerViews.LecturerDashboard>();
            services.AddTransient<Pages.AdminDashboard>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // attempts to auto-login the user
            var authService = ServiceProvider.GetRequiredService<IAuthService>();
            if (await authService.TryAutoLogin())
            {
                var dashboard = ServiceProvider.GetRequiredService<LecturerViews.LecturerDashboard>();
                dashboard.Show();
            }
            else
            {
                var loginPage = ServiceProvider.GetRequiredService<LoginPage>();
                loginPage.Show();
            }
        }
    }
