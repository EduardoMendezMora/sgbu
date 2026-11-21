using SGBU.Services.Seguridad;

namespace SGBU.UI.Navegacion;

/// <summary>
/// Los ViewModels no abren ventanas directamente (eso los ataría a WPF y no se podrían
/// probar). Piden "muestra la principal" a este servicio, y él se encarga de las ventanas.
/// </summary>
public interface IServicioVentanas
{
    void MostrarLogin();

    void MostrarPrincipal(Sesion sesion);

    void CerrarSesion();
}
