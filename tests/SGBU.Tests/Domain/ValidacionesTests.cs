using SGBU.Domain.Excepciones;
using SGBU.Domain.Reglas;
using Xunit;

namespace SGBU.Tests.Domain;

public class ValidacionesTests
{
    [Theory]
    [InlineData("9780132350884")]
    [InlineData("978-0-13-235088-4")]
    [InlineData("978 0 201 63361 0")]
    public void Isbn_AceptaIsbn13ConDigitoVerificadorCorrecto(string valor)
    {
        Assert.True(Isbn.EsValido(valor));
    }

    [Theory]
    [InlineData("9780132350885")] // dígito verificador incorrecto
    [InlineData("978013235088")] // 12 dígitos
    [InlineData("97801323508840")] // 14 dígitos
    [InlineData("978013235088X")] // letra
    [InlineData("")]
    [InlineData(null)]
    public void Isbn_RechazaIsbnInvalido(string? valor)
    {
        Assert.False(Isbn.EsValido(valor));
    }

    [Fact]
    public void Isbn_Normalizar_QuitaGuionesYEspacios()
    {
        Assert.Equal("9780132350884", Isbn.Normalizar("978-0-13 235088-4"));
    }

    [Fact]
    public void Isbn_Normalizar_LanzaExcepcionSiEsInvalido()
    {
        Assert.Throws<DatoInvalidoException>(() => Isbn.Normalizar("123"));
    }

    [Theory]
    [InlineData("ana@sgbu.ac.cr")]
    [InlineData("carlos.jimenez@estudiante.ac.cr")]
    [InlineData("maria_v+biblio@gmail.com")]
    public void Email_AceptaCorreosValidos(string valor)
    {
        Assert.True(Validador.EsEmailValido(valor));
    }

    [Theory]
    [InlineData("ana")]
    [InlineData("ana@")]
    [InlineData("@sgbu.ac.cr")]
    [InlineData("ana@sgbu")]
    [InlineData("ana @sgbu.ac.cr")]
    [InlineData("")]
    public void Email_RechazaCorreosInvalidos(string valor)
    {
        Assert.False(Validador.EsEmailValido(valor));
    }

    [Theory]
    [InlineData("101110111")] // cédula nacional: 9 dígitos
    [InlineData("1-0111-0111")] // cédula con guiones
    [InlineData("155812345678")] // DIMEX: 11 o 12 dígitos
    public void Dni_AceptaCedulaODimex(string valor)
    {
        Assert.True(Validador.EsDniValido(valor));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("A01110111")]
    [InlineData("1011101110")] // 10 dígitos: ni cédula ni DIMEX
    [InlineData("")]
    public void Dni_RechazaFormatosInvalidos(string valor)
    {
        Assert.False(Validador.EsDniValido(valor));
    }

    [Theory]
    [InlineData("88880001")]
    [InlineData("8888-0001")]
    [InlineData("+506 8888-0001")]
    [InlineData("2222-3333")]
    public void Telefono_AceptaNumerosDeCostaRica(string valor)
    {
        Assert.True(Validador.EsTelefonoValido(valor));
    }

    [Theory]
    [InlineData("8888")]
    [InlineData("888800011")]
    [InlineData("telefono")]
    [InlineData("1888-0001")] // los teléfonos de CR no empiezan con 1
    public void Telefono_RechazaNumerosInvalidos(string valor)
    {
        Assert.False(Validador.EsTelefonoValido(valor));
    }
}
