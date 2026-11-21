using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using Xunit;

namespace SGBU.Tests.Domain;

public class MaterialTests
{
    private const string IsbnValido = "9780132350884";

    [Fact]
    public void Libro_SeCreaConStockDisponibleIgualAlTotal()
    {
        var libro = new Libro(IsbnValido, "Clean Code", "Robert C. Martin", 5, "1.ª");

        Assert.Equal(TipoMaterial.Libro, libro.Tipo);
        Assert.Equal(5, libro.StockTotal);
        Assert.Equal(5, libro.StockDisponible);
    }

    [Fact]
    public void Material_NormalizaElIsbn()
    {
        var libro = new Libro("978-0-13-235088-4", "Clean Code", "Robert C. Martin", 1);

        Assert.Equal(IsbnValido, libro.Isbn);
    }

    [Fact]
    public void Material_RechazaIsbnInvalido()
    {
        Assert.Throws<DatoInvalidoException>(() => new Libro("9780132350885", "X", "Y", 1));
    }

    [Theory]
    [InlineData("", "Autor")]
    [InlineData("   ", "Autor")]
    [InlineData("Título", "")]
    public void Material_RechazaTituloOAutorVacios(string titulo, string autor)
    {
        Assert.Throws<DatoInvalidoException>(() => new Libro(IsbnValido, titulo, autor, 1));
    }

    [Fact]
    public void Material_RechazaStockNegativo()
    {
        Assert.Throws<DatoInvalidoException>(() => new Libro(IsbnValido, "X", "Y", -1));
    }

    [Fact]
    public void Prestar_RestaUnoAlStockDisponible()
    {
        var libro = new Libro(IsbnValido, "X", "Y", 2);

        libro.Prestar();

        Assert.Equal(1, libro.StockDisponible);
        Assert.Equal(2, libro.StockTotal);
    }

    [Fact]
    public void Prestar_SinStockLanzaStockInsuficiente()
    {
        var libro = new Libro(IsbnValido, "X", "Y", 1);
        libro.Prestar();

        Assert.Throws<StockInsuficienteException>(() => libro.Prestar());
    }

    [Fact]
    public void Devolver_SumaUnoAlStockDisponible()
    {
        var libro = new Libro(IsbnValido, "X", "Y", 2);
        libro.Prestar();

        libro.Devolver();

        Assert.Equal(2, libro.StockDisponible);
    }

    [Fact]
    public void Devolver_NoPuedeSuperarElStockTotal()
    {
        var libro = new Libro(IsbnValido, "X", "Y", 2);

        Assert.Throws<OperacionNoPermitidaException>(() => libro.Devolver());
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(1, true)]
    [InlineData(4, false)]
    public void EsStockCritico_CuandoQuedanTresOMenos(int stock, bool esperado)
    {
        var libro = new Libro(IsbnValido, "X", "Y", stock);

        Assert.Equal(esperado, libro.EsStockCritico);
    }

    [Fact]
    public void Revista_GuardaVolumenYPeriodicidad()
    {
        var revista = new Revista(IsbnValido, "IEEE Software", "IEEE", 2, "Vol. 42 n.º 2", "Bimestral");

        Assert.Equal(TipoMaterial.Revista, revista.Tipo);
        Assert.Equal("Vol. 42 n.º 2", revista.NumeroVolumen);
        Assert.Equal("Bimestral", revista.Periodicidad);
    }

    [Fact]
    public void Tesis_ExigeUniversidad()
    {
        Assert.Throws<DatoInvalidoException>(() => new Tesis(IsbnValido, "X", "Y", 1, " ", "Licenciatura", "Tutor"));
    }

    [Fact]
    public void Tesis_SeCreaConSusDatos()
    {
        var tesis = new Tesis(IsbnValido, "Sistema de reservas", "Valeria Soto", 1, "Universidad Hispanoamericana", "Licenciatura", "Lic. Marco Ruiz");

        Assert.Equal(TipoMaterial.Tesis, tesis.Tipo);
        Assert.Equal("Universidad Hispanoamericana", tesis.Universidad);
    }

    [Fact]
    public void Material_EsPolimorfico_CadaTipoDescribeSusDatos()
    {
        Material[] materiales =
        [
            new Libro(IsbnValido, "Clean Code", "Martin", 1, "1.ª"),
            new Revista(IsbnValido, "IEEE Software", "IEEE", 1, "Vol. 42", "Bimestral"),
            new Tesis(IsbnValido, "Reservas", "Soto", 1, "UH", "Licenciatura", null),
        ];

        Assert.Equal("Edición 1.ª", materiales[0].Detalle);
        Assert.Equal("Vol. 42 · Bimestral", materiales[1].Detalle);
        Assert.Equal("Licenciatura · UH", materiales[2].Detalle);
    }
}
