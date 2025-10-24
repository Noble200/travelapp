using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using Allva.Desktop.Services;
using Allva.Desktop.ViewModels;
using Allva.Desktop.Views;

namespace Allva.Desktop
{
    public partial class App : Application
    {
        public static IServiceProvider? Services { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Configurar servicios
            ConfigureServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Crear la vista de login
                var loginView = new LoginView();
                
                // Asignar el DataContext (ViewModel)
                loginView.DataContext = new LoginViewModel();

                // Crear ventana principal con LoginView como contenido
                var mainWindow = new Window
                {
                    Title = "Allva System - Login",
                    Width = 900,
                    Height = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    CanResize = true,
                    Content = loginView  // UserControl como contenido
                };

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Registrar servicios singleton
            services.AddSingleton<LocalizationService>();
            services.AddSingleton<NavigationService>();

            // Registrar ViewModels
            services.AddTransient<LoginViewModel>();

            Services = services.BuildServiceProvider();
        }
    }
}