using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Allva.Desktop.Services;

namespace Allva.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel para la pantalla de inicio de sesión
    /// Implementa toda la lógica de autenticación y validación
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        // Nota: Descomentar estas líneas cuando implementes los servicios
        // private readonly IAuthenticationService _authService;
        // private readonly INavigationService _navigationService;
        // private readonly IDialogService _dialogService;

        // ============================================
        // LOCALIZACIÓN
        // ============================================

        public LocalizationService Localization => LocalizationService.Instance;

        // ============================================
        // PROPIEDADES OBSERVABLES
        // ============================================

        [ObservableProperty]
        private string _numeroUsuario = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _codigoLocal = string.Empty;

        [ObservableProperty]
        private bool _recordarSesion = false;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        [ObservableProperty]
        private bool _mostrarError = false;

        [ObservableProperty]
        private int _intentosFallidos = 0;

        [ObservableProperty]
        private bool _usuarioBloqueado = false;

        [ObservableProperty]
        private int _tiempoBloqueoRestante = 0;

        // ============================================
        // CONSTRUCTOR
        // ============================================

        public LoginViewModel()
        {
            // Cuando implementes los servicios, usa este constructor:
            // _authService = authService;
            // _navigationService = navigationService;
            // _dialogService = dialogService;

            // Cargar datos guardados si existe
            CargarDatosGuardados();
        }

        // Constructor con inyección de dependencias (descomentar cuando tengas los servicios)
        /*
        public LoginViewModel(
            IAuthenticationService authService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            CargarDatosGuardados();
        }
        */

        // ============================================
        // COMANDOS
        // ============================================

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                MostrarError = false;
                MensajeError = string.Empty;

                // Validar campos
                if (!ValidarCampos())
                {
                    return;
                }

                // Preparar request
                var loginRequest = new LoginRequest
                {
                    NumeroUsuario = NumeroUsuario.Trim(),
                    Password = Password,
                    CodigoLocal = CodigoLocal.Trim().ToUpper(),
                    UUID = ObtenerUUID(),
                    MAC = ObtenerMACAddress(),
                    IP = ObtenerIPLocal(),
                    UserAgent = ObtenerUserAgent()
                };

                // TODO: Llamar al servicio de autenticación cuando lo implementes
                // var response = await _authService.LoginAsync(loginRequest);

                // ============================================
                // SIMULACIÓN DE LOGIN PARA TESTING
                // ============================================
                await Task.Delay(1500); // Simular delay de red
                
                // Simulación simple: Si el usuario es 9999 y password es Test1234!, login exitoso
                bool loginExitoso = (NumeroUsuario == "9999" && Password == "Test1234!") ||
                                   (NumeroUsuario == "1001" && Password == "Admin123!") ||
                                   (NumeroUsuario == "1002" && Password == "Usuario123!");

                if (loginExitoso)
                {
                    // Resetear intentos fallidos
                    IntentosFallidos = 0;
                    UsuarioBloqueado = false;

                    // Guardar datos si se seleccionó "recordar"
                    if (RecordarSesion)
                    {
                        GuardarDatosLocales();
                    }
                    else
                    {
                        LimpiarDatosGuardados();
                    }

                    // Preparar datos de sesión
                    var loginData = new LoginSuccessData
                    {
                        UserName = GetUserNameFromNumber(NumeroUsuario),
                        UserNumber = NumeroUsuario,
                        LocalCode = CodigoLocal.ToUpper(),
                        UserType = NumeroUsuario == "1001" ? "ADMIN" : "EMPLEADO",
                        Token = $"token-{Guid.NewGuid()}"
                    };

                    // ⭐⭐⭐ NAVEGAR AL TEST PANEL ⭐⭐⭐
                    var navigationService = new NavigationService();
                    navigationService.NavigateToTestPanel(loginData);
                }
                else
                {
                    // Login fallido
                    ManejarLoginFallido("Credenciales incorrectas", "PASSWORD_INCORRECTO");
                }
            }
            catch (Exception ex)
            {
                MensajeError = Localization["Error_Connection"];
                MostrarError = true;
                
                // Log del error
                Console.WriteLine($"Error en login: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RecuperarPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NumeroUsuario))
            {
                // TODO: Usar DialogService cuando lo implementes
                // await _dialogService.ShowWarningAsync(...)
                
                MostrarMensajeError(Localization["Recovery_UserRequiredMessage"]);
                return;
            }

            try
            {
                IsLoading = true;
                
                // TODO: Implementar cuando tengas el servicio
                // var resultado = await _authService.SolicitarRecuperacionPasswordAsync(NumeroUsuario);
                
                // Simulación
                await Task.Delay(1000);
                MostrarMensajeError(Localization["Recovery_SuccessMessage"]);
            }
            catch (Exception ex)
            {
                MostrarMensajeError(Localization["Recovery_ErrorGeneric"]);
                Console.WriteLine($"Error en recuperación: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void LimpiarFormulario()
        {
            NumeroUsuario = string.Empty;
            Password = string.Empty;
            CodigoLocal = string.Empty;
            MostrarError = false;
            MensajeError = string.Empty;
        }

        [RelayCommand]
        private void CambiarIdioma()
        {
            Localization.ToggleLanguage();
            
            // Si hay un mensaje de error, actualizarlo al nuevo idioma
            if (MostrarError && !string.IsNullOrEmpty(MensajeError))
            {
                // Recargar el mensaje de error en el nuevo idioma
                MostrarError = false;
                MensajeError = string.Empty;
            }
        }

        // ============================================
        // MÉTODOS DE VALIDACIÓN
        // ============================================

        private bool CanLogin()
        {
            return !IsLoading && 
                   !UsuarioBloqueado &&
                   !string.IsNullOrWhiteSpace(NumeroUsuario) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(CodigoLocal);
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(NumeroUsuario))
            {
                MostrarMensajeError(Localization["Error_UserRequired"]);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MostrarMensajeError(Localization["Error_PasswordRequired"]);
                return false;
            }

            if (Password.Length < 8)
            {
                MostrarMensajeError(Localization["Error_PasswordMinLength"]);
                return false;
            }

            if (string.IsNullOrWhiteSpace(CodigoLocal))
            {
                MostrarMensajeError(Localization["Error_OfficeRequired"]);
                return false;
            }

            return true;
        }

        // ============================================
        // MANEJO DE ERRORES
        // ============================================

        private void ManejarLoginFallido(string error, string motivoFallo)
        {
            IntentosFallidos++;

            switch (motivoFallo)
            {
                case "USUARIO_NO_ENCONTRADO":
                    MostrarMensajeError(Localization["Error_UserNotFound"]);
                    break;

                case "PASSWORD_INCORRECTO":
                    if (IntentosFallidos >= 5)
                    {
                        BloquearUsuarioTemporalmente();
                    }
                    else
                    {
                        int intentosRestantes = 5 - IntentosFallidos;
                        MostrarMensajeError(Localization.GetText("Error_PasswordIncorrect", intentosRestantes));
                    }
                    break;

                case "USUARIO_BLOQUEADO":
                    BloquearUsuarioTemporalmente();
                    break;

                case "LOCAL_INVALIDO":
                    MostrarMensajeError(Localization["Error_OfficeInvalid"]);
                    break;

                case "SIN_PERMISO_LOCAL":
                    MostrarMensajeError(Localization["Error_NoPermission"]);
                    break;

                case "COMERCIO_INACTIVO":
                    MostrarMensajeError(Localization["Error_CompanyInactive"]);
                    break;

                case "DISPOSITIVO_NO_AUTORIZADO":
                    MostrarMensajeError(Localization["Error_DeviceUnauthorized"]);
                    break;

                default:
                    MostrarMensajeError(error ?? Localization["Error_Generic"]);
                    break;
            }
        }

        private void BloquearUsuarioTemporalmente()
        {
            UsuarioBloqueado = true;
            TiempoBloqueoRestante = 15; // 15 minutos

            MostrarMensajeError(Localization.GetText("Error_UserBlocked", TiempoBloqueoRestante));

            // Iniciar countdown
            IniciarContadorBloqueo();
        }

        private async void IniciarContadorBloqueo()
        {
            while (TiempoBloqueoRestante > 0)
            {
                await Task.Delay(60000); // 1 minuto
                TiempoBloqueoRestante--;

                if (TiempoBloqueoRestante > 0)
                {
                    MostrarMensajeError(Localization.GetText("Error_UserBlockedRemaining", TiempoBloqueoRestante));
                }
            }

            // Desbloquear
            UsuarioBloqueado = false;
            IntentosFallidos = 0;
            MostrarError = false;
            MensajeError = string.Empty;
        }

        private void MostrarMensajeError(string mensaje)
        {
            MensajeError = mensaje;
            MostrarError = true;
        }

        // ============================================
        // PERSISTENCIA LOCAL
        // ============================================

        private void GuardarDatosLocales()
        {
            try
            {
                Preferences.Set("last_usuario", NumeroUsuario);
                Preferences.Set("last_local", CodigoLocal);
                Preferences.Set("recordar_sesion", RecordarSesion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando preferencias: {ex.Message}");
            }
        }

        private void CargarDatosGuardados()
        {
            try
            {
                bool recordar = Preferences.Get("recordar_sesion", false);
                
                if (recordar)
                {
                    NumeroUsuario = Preferences.Get("last_usuario", string.Empty);
                    CodigoLocal = Preferences.Get("last_local", string.Empty);
                    RecordarSesion = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando preferencias: {ex.Message}");
            }
        }

        private void LimpiarDatosGuardados()
        {
            try
            {
                Preferences.Remove("last_usuario");
                Preferences.Remove("last_local");
                Preferences.Remove("recordar_sesion");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error limpiando preferencias: {ex.Message}");
            }
        }

        // ============================================
        // INFORMACIÓN DEL DISPOSITIVO
        // ============================================

        private string ObtenerUUID()
        {
            try
            {
                var uuid = Preferences.Get("device_uuid", string.Empty);
                
                if (string.IsNullOrEmpty(uuid))
                {
                    uuid = Guid.NewGuid().ToString();
                    Preferences.Set("device_uuid", uuid);
                }
                
                return uuid;
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        private string ObtenerMACAddress()
        {
            try
            {
                var networkInterface = System.Net.NetworkInformation.NetworkInterface
                    .GetAllNetworkInterfaces()
                    .FirstOrDefault(nic => 
                        nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                        nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback);

                return networkInterface?.GetPhysicalAddress().ToString() ?? "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        private string ObtenerIPLocal()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                var ip = host.AddressList
                    .FirstOrDefault(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                
                return ip?.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        private string ObtenerUserAgent()
        {
            try
            {
                var os = Environment.OSVersion;
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return $"AllvaDesktop/{version} ({os.Platform} {os.Version})";
            }
            catch
            {
                return "AllvaDesktop/1.0";
            }
        }

        private string GetUserNameFromNumber(string numeroUsuario)
        {
            // Mapeo simple de números a nombres para la demo
            return numeroUsuario switch
            {
                "1001" => "Juan Pérez",
                "1002" => "María González",
                "1003" => "Carlos Rodríguez",
                "1004" => "Ana Martínez",
                "9999" => "Test User",
                _ => $"Usuario {numeroUsuario}"
            };
        }

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================

        partial void OnNumeroUsuarioChanged(string value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }

        partial void OnPasswordChanged(string value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }

        partial void OnCodigoLocalChanged(string value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }
    }

    // ============================================
    // MODELOS AUXILIARES
    // ============================================

    /// <summary>
    /// Modelo para la solicitud de login
    /// </summary>
    public class LoginRequest
    {
        public string NumeroUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CodigoLocal { get; set; } = string.Empty;
        public string UUID { get; set; } = string.Empty;
        public string MAC { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para la respuesta de login
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string MotivoFallo { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public bool RequiereCambioPassword { get; set; }
    }

    // ============================================
    // CLASE AUXILIAR: Preferences
    // ============================================
    
    /// <summary>
    /// Clase estática para manejar preferencias locales
    /// Guarda datos en el sistema de archivos
    /// </summary>
    public static class Preferences
    {
        private static readonly string PreferencesPath;
        private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        static Preferences()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var allvaFolder = System.IO.Path.Combine(appData, "Allva");
            
            if (!System.IO.Directory.Exists(allvaFolder))
            {
                System.IO.Directory.CreateDirectory(allvaFolder);
            }

            PreferencesPath = System.IO.Path.Combine(allvaFolder, "preferences.dat");
            CargarPreferencias();
        }

        private static void CargarPreferencias()
        {
            try
            {
                if (System.IO.File.Exists(PreferencesPath))
                {
                    var lines = System.IO.File.ReadAllLines(PreferencesPath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            _cache[parts[0]] = parts[1];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando preferencias: {ex.Message}");
            }
        }

        private static void GuardarPreferencias()
        {
            try
            {
                var lines = _cache.Select(kvp => $"{kvp.Key}={kvp.Value}");
                System.IO.File.WriteAllLines(PreferencesPath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando preferencias: {ex.Message}");
            }
        }

        public static void Set(string key, string value)
        {
            _cache[key] = value;
            GuardarPreferencias();
        }

        public static void Set(string key, bool value)
        {
            _cache[key] = value.ToString();
            GuardarPreferencias();
        }

        public static string Get(string key, string defaultValue)
        {
            return _cache.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static bool Get(string key, bool defaultValue)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return bool.TryParse(value, out var result) ? result : defaultValue;
            }
            return defaultValue;
        }

        public static void Remove(string key)
        {
            _cache.Remove(key);
            GuardarPreferencias();
        }
    }
}