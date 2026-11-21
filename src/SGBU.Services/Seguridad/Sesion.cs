namespace SGBU.Services.Seguridad;

/// <summary>
/// El usuario que inició sesión. Los permisos por rol viven aquí, en un solo lugar:
/// la UI pregunta sesion.Puede(Permiso.X) para mostrar u ocultar opciones, y los
/// servicios lo vuelven a revisar antes de cada operación (nunca confiar solo en la UI).
/// </summary>
public sealed record Sesion(int UsuarioId, string NombreCompleto, string Rol, DateTime Inicio)
{
    private static readonly Dictionary<string, HashSet<Permiso>> PermisosPorRol = new(StringComparer.Ordinal)
    {
        ["Administrador"] = [.. Enum.GetValues<Permiso>()],
        ["Profesor"] = [Permiso.VerCatalogo, Permiso.VerMisPrestamos, Permiso.VerReportes],
        ["Estudiante"] = [Permiso.VerCatalogo, Permiso.VerMisPrestamos],
    };

    public bool Puede(Permiso permiso) =>
        PermisosPorRol.TryGetValue(Rol, out var permisos) && permisos.Contains(permiso);
}
