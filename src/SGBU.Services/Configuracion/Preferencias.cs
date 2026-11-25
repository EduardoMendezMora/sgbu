using System.Text.Json;

namespace SGBU.Services.Configuracion;

// Tema e idioma elegidos. Se guardan en un archivo JSON en la carpeta del usuario de Windows
// (AppData\SGBU\configuracion.json), así cada computadora recuerda su configuración.
public class Preferencias
{
    public string Tema { get; set; } = "Claro";

    public string Idioma { get; set; } = "es";

    public static string RutaPorDefecto()
    {
        string carpeta = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(carpeta, "SGBU", "configuracion.json");
    }

    public static Preferencias Cargar(string ruta)
    {
        Preferencias preferencias = new Preferencias();

        if (File.Exists(ruta))
        {
            try
            {
                Preferencias? leidas = JsonSerializer.Deserialize<Preferencias>(File.ReadAllText(ruta));
                if (leidas != null)
                {
                    preferencias = leidas;
                }
            }
            catch (JsonException)
            {
                // Si el archivo está dañado se usan los valores por defecto.
            }
        }

        // Solo se aceptan los valores conocidos.
        if (preferencias.Tema != "Claro" && preferencias.Tema != "Oscuro")
        {
            preferencias.Tema = "Claro";
        }

        if (preferencias.Idioma != "es" && preferencias.Idioma != "en")
        {
            preferencias.Idioma = "es";
        }

        return preferencias;
    }

    public void Guardar(string ruta)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        File.WriteAllText(ruta, JsonSerializer.Serialize(this));
    }
}
