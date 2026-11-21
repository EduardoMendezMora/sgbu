namespace SGBU.UI.ViewModels;

// Un elemento de una lista desplegable: el valor que se guarda y el nombre que se muestra.
public class Opcion
{
    public Opcion(string valor, string nombre)
    {
        Valor = valor;
        Nombre = nombre;
    }

    public string Valor { get; }

    public string Nombre { get; }

    public override string ToString()
    {
        return Nombre;
    }
}
