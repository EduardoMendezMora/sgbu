using SGBU.Services.Seguridad;

namespace SGBU.UI.ViewModels;

// Una opción del menú y el permiso que se necesita para verla.
public class OpcionMenu
{
    public OpcionMenu(string titulo, Permiso permiso)
    {
        Titulo = titulo;
        Permiso = permiso;
    }

    public string Titulo { get; }

    public Permiso Permiso { get; }

    public override string ToString()
    {
        return Titulo;
    }
}
