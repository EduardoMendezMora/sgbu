using SGBU.Services.Seguridad;

namespace SGBU.UI.Navegacion;

public interface IServicioVentanas
{
    void MostrarLogin();

    void MostrarPrincipal(Sesion sesion);

    void CerrarSesion();
}
