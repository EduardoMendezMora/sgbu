using SGBU.Services.Seguridad;

namespace SGBU.UI.ViewModels;

/// <summary>Una opción del menú lateral y el permiso que necesita.</summary>
public sealed record OpcionMenu(string Titulo, Permiso Permiso);
