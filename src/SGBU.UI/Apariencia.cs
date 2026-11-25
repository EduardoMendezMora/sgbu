using System.Windows;
using SGBU.Services.Configuracion;

namespace SGBU.UI;

// Cambia el tema (colores) y el idioma (textos) de toda la aplicación.
// Son dos diccionarios de recursos que se cargan en App.xaml: Temas/... y Idiomas/...
public static class Apariencia
{
    public static void Aplicar(Preferencias preferencias)
    {
        ResourceDictionary tema = new ResourceDictionary();
        tema.Source = new Uri("pack://application:,,,/Temas/" + preferencias.Tema + ".xaml");

        ResourceDictionary idioma = new ResourceDictionary();
        idioma.Source = new Uri("pack://application:,,,/Idiomas/" + preferencias.Idioma + ".xaml");

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(tema);
        Application.Current.Resources.MergedDictionaries.Add(idioma);
    }
}
