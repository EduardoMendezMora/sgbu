using SGBU.Services.Configuracion;
using Xunit;

namespace SGBU.Tests.Services;

public class PreferenciasTests
{
    [Fact]
    public void Cargar_SiNoExisteElArchivo_UsaClaroYEspanol()
    {
        var preferencias = Preferencias.Cargar(Path.Combine(Path.GetTempPath(), "no-existe-" + Guid.NewGuid() + ".json"));

        Assert.Equal("Claro", preferencias.Tema);
        Assert.Equal("es", preferencias.Idioma);
    }

    [Fact]
    public void GuardarYCargar_ConservaTemaEIdioma()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "sgbu-" + Guid.NewGuid(), "configuracion.json");
        var preferencias = new Preferencias { Tema = "Oscuro", Idioma = "en" };

        preferencias.Guardar(ruta);
        var leidas = Preferencias.Cargar(ruta);

        Assert.Equal("Oscuro", leidas.Tema);
        Assert.Equal("en", leidas.Idioma);
        Directory.Delete(Path.GetDirectoryName(ruta)!, true);
    }

    [Fact]
    public void Cargar_ArchivoDanado_UsaLosValoresPorDefecto()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "sgbu-danado-" + Guid.NewGuid() + ".json");
        File.WriteAllText(ruta, "{ esto no es json");

        var preferencias = Preferencias.Cargar(ruta);

        Assert.Equal("Claro", preferencias.Tema);
        File.Delete(ruta);
    }

    [Fact]
    public void Cargar_ValoresDesconocidos_SeCorrigen()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "sgbu-raro-" + Guid.NewGuid() + ".json");
        File.WriteAllText(ruta, "{\"Tema\":\"Rosado\",\"Idioma\":\"fr\"}");

        var preferencias = Preferencias.Cargar(ruta);

        Assert.Equal("Claro", preferencias.Tema);
        Assert.Equal("es", preferencias.Idioma);
        File.Delete(ruta);
    }
}
