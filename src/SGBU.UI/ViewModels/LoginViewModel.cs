using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Excepciones;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ServicioAutenticacion autenticacion;
    private readonly IServicioVentanas ventanas;

    // [ObservableProperty] crea la propiedad pública (Usuario, MensajeError)
    // y avisa a la vista cada vez que cambia.
    [ObservableProperty]
    private string usuario = string.Empty;

    [ObservableProperty]
    private string mensajeError = string.Empty;

    public LoginViewModel(ServicioAutenticacion autenticacion, IServicioVentanas ventanas)
    {
        this.autenticacion = autenticacion;
        this.ventanas = ventanas;
    }

    // El PasswordBox no permite Binding, por eso la vista copia la clave aquí.
    public string Contrasena { get; set; } = string.Empty;

    // [RelayCommand] crea IniciarSesionCommand, que es lo que ejecuta el botón.
    [RelayCommand]
    private async Task IniciarSesion()
    {
        MensajeError = string.Empty;

        try
        {
            Sesion sesion = await autenticacion.IniciarSesionAsync(Usuario, Contrasena);
            ventanas.MostrarPrincipal(sesion);
        }
        catch (ExcepcionDominio ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo conectar con la base de datos.";
        }
    }
}
