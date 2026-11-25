using System.Windows;

namespace SGBU.UI;

// Busca un texto del idioma elegido (Idiomas/es.xaml o en.xaml) desde el código C#.
public static class Textos
{
    public static string Texto(string clave)
    {
        if (Application.Current.Resources[clave] is string texto)
        {
            return texto;
        }

        return clave;
    }
}
