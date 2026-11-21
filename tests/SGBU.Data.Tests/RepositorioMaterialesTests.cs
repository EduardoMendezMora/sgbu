using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using Xunit;

namespace SGBU.Data.Tests;

public class RepositorioMaterialesTests
{
    [FactConBase]
    public async Task ObtenerPorIsbn_DevuelveUnLibroConSusDatos()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var material = await uow.Materiales.ObtenerPorIsbnAsync("9780132350884");

        var libro = Assert.IsType<Libro>(material);
        Assert.Equal("Clean Code", libro.Titulo);
        Assert.Equal("Prentice Hall", libro.Editorial);
        Assert.Equal((short)2008, libro.AnioPublicacion);
        Assert.Equal(5, libro.StockTotal);
        Assert.Equal("1.ª", libro.Edicion);
    }

    [FactConBase]
    public async Task ObtenerPorIsbn_CreaLaSubclaseSegunLaColumnaTipo()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var revista = Assert.IsType<Revista>(await uow.Materiales.ObtenerPorIsbnAsync("9781234567897"));
        var tesis = Assert.IsType<Tesis>(await uow.Materiales.ObtenerPorIsbnAsync("9783333444451"));

        Assert.NotNull(revista.Periodicidad);
        Assert.False(string.IsNullOrWhiteSpace(tesis.Universidad));
    }

    [FactConBase]
    public async Task ObtenerPorIsbn_AceptaGuionesYDevuelveNullSiNoExiste()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        Assert.NotNull(await uow.Materiales.ObtenerPorIsbnAsync("978-0-13-235088-4"));
        Assert.Null(await uow.Materiales.ObtenerPorIsbnAsync("9780000000002"));
    }

    [FactConBase]
    public async Task Buscar_PaginaEnElServidorYDevuelveElTotal()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var pagina1 = await uow.Materiales.BuscarAsync(null, null, numeroPagina: 1, tamanoPagina: 5);
        var pagina3 = await uow.Materiales.BuscarAsync(null, null, numeroPagina: 3, tamanoPagina: 5);

        Assert.Equal(5, pagina1.Elementos.Count);
        Assert.True(pagina1.Total >= 14);
        Assert.True(pagina1.HaySiguiente);
        Assert.Empty(pagina1.Elementos.Select(m => m.Id).Intersect(pagina3.Elementos.Select(m => m.Id)));
    }

    [FactConBase]
    public async Task Buscar_FiltraPorTextoYTipo()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var porAutor = await uow.Materiales.BuscarAsync("tanenbaum", null, 1, 10);
        var tesis = await uow.Materiales.BuscarAsync(null, TipoMaterial.Tesis, 1, 10);

        Assert.Single(porAutor.Elementos);
        Assert.Equal(3, tesis.Total);
        Assert.All(tesis.Elementos, m => Assert.IsType<Tesis>(m));
    }

    [FactConBase]
    public async Task Agregar_YLuegoLeer_DevuelveLoMismo()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var nueva = new Tesis("9780000000019", "Tesis de prueba", "Autor Prueba", 2, "UH", "Licenciatura", "Tutor X")
        {
            Categoria = "Pruebas",
            AnioPublicacion = 2026,
        };

        var id = await uow.Materiales.AgregarAsync(nueva);
        var leida = Assert.IsType<Tesis>(await uow.Materiales.ObtenerPorIdAsync(id));

        Assert.Equal("UH", leida.Universidad);
        Assert.Equal("Tutor X", leida.Tutor);
        Assert.Equal(2, leida.StockDisponible);
    }

    [FactConBase]
    public async Task Agregar_IsbnRepetido_LanzaExcepcionDeDominio()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var repetido = new Libro("9780132350884", "Otro", "Otro", 1);

        var ex = await Assert.ThrowsAsync<DatoInvalidoException>(() => uow.Materiales.AgregarAsync(repetido));
        Assert.Contains("ISBN", ex.Message, StringComparison.Ordinal);
    }

    [FactConBase]
    public async Task Actualizar_GuardaElStockDespuesDePrestar()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var material = (await uow.Materiales.ObtenerPorIsbnAsync("9780134685991"))!;

        material.Prestar();
        await uow.Materiales.ActualizarAsync(material);

        var otraVez = await uow.Materiales.ObtenerPorIdAsync(material.Id);
        Assert.Equal(material.StockTotal - 1, otraVez!.StockDisponible);
    }
}
