using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Services.Catalogo;
using SGBU.Services.Seguridad;
using Xunit;

namespace SGBU.Tests.Services;

public class ServicioCatalogoTests
{
    private static readonly Sesion Admin = new(1, "Ana Rojas", "Administrador", DateTime.Now);
    private static readonly Sesion Estudiante = new(4, "Carlos Jiménez", "Estudiante", DateTime.Now);

    [Fact]
    public async Task Buscar_DevuelveLaPaginaPedida()
    {
        var (servicio, _) = Crear();

        var pagina = await servicio.BuscarAsync("code", null, 1);

        Assert.Single(pagina.Elementos);
        Assert.Equal(1, pagina.Total);
    }

    [Fact]
    public async Task Guardar_Nuevo_AgregaElLibroYAudita()
    {
        var (servicio, fabrica) = Crear();
        var datos = new FormularioMaterial { Tipo = TipoMaterial.Libro, Isbn = "978-0-201-63361-0", Titulo = "Design Patterns", Autor = "GoF", StockTotal = 3, Edicion = "1.ª" };

        await servicio.GuardarAsync(Admin, datos);

        var libro = Assert.IsType<Libro>(fabrica.Materiales.Last());
        Assert.Equal("9780201633610", libro.Isbn);
        Assert.Equal(3, libro.StockDisponible);
        Assert.Equal("MATERIAL_CREADO", Assert.Single(fabrica.Auditoria).Accion);
    }

    [Fact]
    public async Task Guardar_NuevaTesis_UsaSusCamposPropios()
    {
        var (servicio, fabrica) = Crear();
        var datos = new FormularioMaterial
        {
            Tipo = TipoMaterial.Tesis, Isbn = "9783333444451", Titulo = "Tesis", Autor = "Ana", StockTotal = 1,
            Universidad = "UH", GradoAcademico = "Licenciatura", Tutor = "Jonathan",
        };

        await servicio.GuardarAsync(Admin, datos);

        var tesis = Assert.IsType<Tesis>(fabrica.Materiales.Last());
        Assert.Equal("UH", tesis.Universidad);
    }

    [Fact]
    public async Task Guardar_Edicion_AjustaElDisponibleSegunLosPrestados()
    {
        // Clean Code: 5 en total, 4 disponibles (1 prestado). Si el total pasa a 8, quedan 7 disponibles.
        var (servicio, fabrica) = Crear();
        var datos = FormularioMaterial.Desde(fabrica.Materiales[0]);
        datos.StockTotal = 8;

        await servicio.GuardarAsync(Admin, datos);

        var editado = fabrica.Materiales.Single(m => m.Id == 1);
        Assert.Equal(8, editado.StockTotal);
        Assert.Equal(7, editado.StockDisponible);
        Assert.True(editado.Activo);
        Assert.Equal("MATERIAL_EDITADO", Assert.Single(fabrica.Auditoria).Accion);
    }

    [Fact]
    public async Task Guardar_Edicion_NoPermiteBajarElTotalPorDebajoDeLosPrestados()
    {
        var (servicio, fabrica) = Crear();
        var datos = FormularioMaterial.Desde(fabrica.Materiales[0]);
        datos.StockTotal = 0;

        await Assert.ThrowsAsync<DatoInvalidoException>(() => servicio.GuardarAsync(Admin, datos));
    }

    [Fact]
    public async Task Guardar_IsbnInvalido_LanzaDatoInvalido()
    {
        var (servicio, _) = Crear();
        var datos = new FormularioMaterial { Tipo = TipoMaterial.Libro, Isbn = "123", Titulo = "X", Autor = "Y", StockTotal = 1 };

        await Assert.ThrowsAsync<DatoInvalidoException>(() => servicio.GuardarAsync(Admin, datos));
    }

    [Fact]
    public async Task Guardar_EstudianteNoPuedeEditarElCatalogo()
    {
        var (servicio, fabrica) = Crear();
        var datos = FormularioMaterial.Desde(fabrica.Materiales[0]);

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.GuardarAsync(Estudiante, datos));
        Assert.Empty(fabrica.Auditoria);
    }

    [Fact]
    public void Formulario_Desde_CopiaLosDatosDeUnaRevista()
    {
        var revista = new Revista(9, "9781234567897", "CACM", "ACM", 3, 2, "Vol. 68", "Mensual") { Editorial = "ACM", AnioPublicacion = 2025 };

        var datos = FormularioMaterial.Desde(revista);

        Assert.Equal(TipoMaterial.Revista, datos.Tipo);
        Assert.Equal("Vol. 68", datos.NumeroVolumen);
        Assert.Equal("Mensual", datos.Periodicidad);
        Assert.Equal((short)2025, datos.AnioPublicacion);
    }

    private static (ServicioCatalogo Servicio, FabricaFalsa Fabrica) Crear()
    {
        var fabrica = new FabricaFalsa();
        fabrica.Materiales.Add(new Libro(1, "9780132350884", "Clean Code", "Robert C. Martin", 5, 4, "1.ª"));
        fabrica.Materiales.Add(new Revista(2, "9781111222239", "IEEE Software", "IEEE", 2, 2, null, "Bimestral"));
        return (new ServicioCatalogo(fabrica), fabrica);
    }
}
