using Jahoot.Display.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Windows;

namespace Jahoot.Display
{
    /// <summary>
    /// This is the main brain of our app! It sets up everything when the app starts,
    /// like all the services and the main window.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// This lets us get hold of all the cool services we've set up.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// When the app first starts, this code runs. It gets all our services ready.
        /// </summary>
        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection); // Set up all the services.
            ServiceProvider = serviceCollection.BuildServiceProvider(); // Build the thing that gives us services.
        }

        /// <summary>
        /// Here's where we tell the app what services it has and how to make them.
        /// </summary>
        /// <param name="services">The list where we add all our services.</param>
        private void ConfigureServices(IServiceCollection services)
        {
            // Add logging so we can see messages in the debug output.
            services.AddLogging(configure => configure.AddDebug());
            // Our secret storage service, only one copy needed.
            services.AddSingleton<ISecureStorageService, SecureStorageService>();
            // The tool for talking to the web server, only one copy needed.
            services.AddSingleton<HttpClient>(new HttpClient
            {
                BaseAddress = new Uri("http://localhost") // This is where our web API lives.
            });
            // Our login/logout service, make a new one each time it's asked for.
            services.AddTransient<IAuthService, AuthService>();
            // Our main window, make a new one each time it's asked for.
            services.AddTransient<MainWindow>();
        }

        /// <summary>
        /// This runs right after the app starts. It shows our main window.
        /// </summary>
        /// <param name="e">Info about the startup event.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Get our main window from the service provider and show it!
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
