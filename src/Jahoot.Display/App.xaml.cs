    using Jahoot.Display.Services;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using System.Windows;

    namespace Jahoot.Display;

    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISecureStorageService, SecureStorageService>();
            services.AddSingleton<HttpClient>(new HttpClient
            {
                BaseAddress = new Uri("http://localhost")
            });
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LecturerViews.LecturerDashboard>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginPage = ServiceProvider.GetRequiredService<LoginPage>();
            loginPage.Show();
        }
    }
