    using Jahoot.Display.Services;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using System.Windows;

    namespace Jahoot.Display;

    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        private const string ClassBaseAddress = "http://localhost";

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISecureStorageService, SecureStorageService>();
            services.AddSingleton<HttpClient>(new HttpClient
            {
                BaseAddress = new Uri(ClassBaseAddress)
            });
            services.AddSingleton<IHttpService, HttpService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ISubjectService, SubjectService>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LecturerViews.LecturerDashboard>();
            services.AddTransient<Pages.AdminDashboard>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginPage = ServiceProvider.GetRequiredService<LoginPage>();
            loginPage.Show();
        }
    }
