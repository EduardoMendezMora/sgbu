using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Excepciones;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

/// <summary>
/// ViewModel del login. No conoce la ventana: expone propiedades y un comando, y la vista
/// se "amarra" a ellos con Binding. CommunityToolkit.Mvvm genera el código repetitivo:
/// [ObservableProperty] crea la propiedad que avisa a la vista cuando cambia, y
/// [RelayCommand] crea el ICommand que el botón ejecuta.
/// </summary>
public sealed partial class LoginViewModel : ObservableObject
{
    private readonly ServicioAutenticacion autenticacion;
    private readonly IServicioVentanas ventanas;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IniciarSesionCommand))]
    private string usuario = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HayError))]
    private string mensajeError = string.Empty;

    [ObservableProperty]
    private bool ocupado;

    public LoginViewModel(ServicioAutenticacion autenticacion, IServicioVentanas ventanas)
    {
        this.autenticacion = autenticacion;
        this.ventanas = ventanas;
    }

    /// <summary>
    /// El PasswordBox de WPF no permite Binding (por seguridad no expone la clave como
    /// propiedad enlazable); la vista la copia aquí en su evento PasswordChanged.
    /// </summary>
    public string Contrasena { get; set; } = string.Empty;

    public bool HayError => MensajeError.Length > 0;

    private bool PuedeIniciarSesion() => !string.IsNullOrWhiteSpace(Usuario);

    [RelayCommand(CanExecute = nameof(PuedeIniciarSesion))]
    private async Task IniciarSesionAsync()
    {
        MensajeError = string.Empty;
        Ocupado = true;
        try
        {
            var sesion = await autenticacion.IniciarSesionAsync(Usuario, Contrasena);
            ventanas.MostrarPrincipal(sesion);
        }
        catch (ExcepcionDominio ex)
        {
            // Errores esperados (clave incorrecta, usuario inactivo): su mensaje ya es para el usuario.
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            // Lo inesperado (sin conexión a la base, etc.): mensaje genérico, sin detalles técnicos.
            MensajeError = "No se pudo conectar con la base de datos. Revise la configuración e intente de nuevo.";
        }
        finally
        {
            Ocupado = false;
        }
    }
}
