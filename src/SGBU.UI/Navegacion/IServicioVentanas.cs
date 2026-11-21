using SGBU.Services.Seguridad;

namespace SGBU.UI.Navegacion;

public interface IServicioVentanas
{
    void MostrarLogin();

    void MostrarPrincipal(Sesion sesion);

    void CerrarSesion();

    // Abre la ventana "Guardar como". Devuelve null si el usuario cancela.
    string? PedirRutaParaGuardar(string nombreSugerido, string filtro);
}
