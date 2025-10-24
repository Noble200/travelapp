using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Allva.Desktop.Views;

namespace Allva.Desktop.Services
{
    /// <summary>
    /// Servicio de navegación para cambiar entre vistas
    /// </summary>
    public class NavigationService
    {
        public event EventHandler<object>? NavigationRequested;

        /// <summary>
        /// Navega a una vista específica
        /// </summary>
        public void NavigateTo(string viewName, object? parameter = null)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow == null) return;

                UserControl? newView = viewName switch
                {
                    "Login" => new LoginView(),
                    "TestPanel" => new TestPanelView(),
                    _ => null
                };

                if (newView != null)
                {
                    mainWindow.Content = newView;
                    
                    // Actualizar título de la ventana
                    mainWindow.Title = viewName switch
                    {
                        "Login" => "Allva System - Login",
                        "TestPanel" => "Allva System - Test Panel",
                        _ => "Allva System"
                    };

                    NavigationRequested?.Invoke(this, newView);
                }
            }
        }

        /// <summary>
        /// Navega al TestPanel después de un login exitoso
        /// </summary>
        public void NavigateToTestPanel(LoginSuccessData loginData)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow == null) return;

                var testPanelView = new TestPanelView();
                
                // Si TestPanelView tiene DataContext, configurarlo aquí
                // testPanelView.DataContext = new TestPanelViewModel 
                // {
                //     UserName = loginData.UserName,
                //     UserNumber = loginData.UserNumber,
                //     LocalCode = loginData.LocalCode,
                //     UserType = loginData.UserType
                // };

                mainWindow.Content = testPanelView;
                mainWindow.Title = "Allva System - Test Panel";

                NavigationRequested?.Invoke(this, testPanelView);
            }
        }

        /// <summary>
        /// Vuelve al login (logout)
        /// </summary>
        public void NavigateToLogin()
        {
            NavigateTo("Login");
        }
    }

    /// <summary>
    /// Datos del login exitoso
    /// </summary>
    public class LoginSuccessData
    {
        public string UserName { get; set; } = string.Empty;
        public string UserNumber { get; set; } = string.Empty;
        public string LocalCode { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}