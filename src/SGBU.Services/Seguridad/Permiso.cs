namespace SGBU.Services.Seguridad;

/// <summary>Lo que cada pantalla necesita para mostrarse.</summary>
public enum Permiso
{
    VerCatalogo,
    GestionarCatalogo,
    GestionarUsuarios,
    RegistrarPrestamos,
    VerMisPrestamos,
    GestionarMultas,
    VerReportes,
    VerAuditoria,
    Configurar,

    // Tema e idioma: cada usuario elige los suyos.
    Preferencias,
}
