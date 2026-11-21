namespace SGBU.Services.Seguridad;

// Datos del usuario que inició sesión y qué puede hacer según su rol.
public class Sesion
{
    public Sesion(int usuarioId, string nombreCompleto, string rol, DateTime inicio)
    {
        UsuarioId = usuarioId;
        NombreCompleto = nombreCompleto;
        Rol = rol;
        Inicio = inicio;
    }

    public int UsuarioId { get; }

    public string NombreCompleto { get; }

    public string Rol { get; }

    public DateTime Inicio { get; }

    public bool Puede(Permiso permiso)
    {
        switch (Rol)
        {
            case "Administrador":
                return true;

            case "Profesor":
                return permiso == Permiso.VerCatalogo
                    || permiso == Permiso.VerMisPrestamos
                    || permiso == Permiso.VerReportes
                    || permiso == Permiso.Preferencias;

            case "Estudiante":
                return permiso == Permiso.VerCatalogo
                    || permiso == Permiso.VerMisPrestamos
                    || permiso == Permiso.Preferencias;

            default:
                return false;
        }
    }
}
