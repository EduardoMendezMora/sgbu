using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using Xunit;

namespace SGBU.Tests.Domain;

public class PrestamoTests
{
    private static readonly DateTime Hoy = new(2026, 11, 20, 10, 0, 0);

    [Fact]
    public void Crear_CalculaElVencimientoSegunLosDiasDelRol()
    {
        var prestamo = Prestamo.Crear(usuarioId: 10, materialId: 20, Estudiante(), Hoy);

        Assert.Equal(new DateOnly(2026, 11, 27), prestamo.FechaVencimiento);
        Assert.True(prestamo.EstaActivo);
        Assert.False(prestamo.Renovado);
    }

    [Fact]
    public void DiasAtraso_EsCeroSiSeDevuelveATiempo()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);

        Assert.Equal(0, prestamo.DiasAtraso(new DateOnly(2026, 11, 27)));
    }

    [Fact]
    public void DiasAtraso_CuentaLosDiasDespuesDelVencimiento()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);

        Assert.Equal(3, prestamo.DiasAtraso(new DateOnly(2026, 11, 30)));
    }

    [Fact]
    public void Devolver_RegistraLaFechaYDevuelveLosDiasDeAtraso()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);

        var dias = prestamo.Devolver(new DateTime(2026, 12, 1, 9, 0, 0));

        Assert.Equal(4, dias);
        Assert.False(prestamo.EstaActivo);
    }

    [Fact]
    public void Devolver_DosVecesNoSePermite()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);
        prestamo.Devolver(Hoy.AddDays(1));

        Assert.Throws<OperacionNoPermitidaException>(() => prestamo.Devolver(Hoy.AddDays(2)));
    }

    [Fact]
    public void Renovar_ExtiendeElVencimientoUnaVez()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);

        prestamo.Renovar(Estudiante(), materialReservado: false, Hoy.AddDays(3));

        Assert.Equal(new DateOnly(2026, 12, 4), prestamo.FechaVencimiento);
        Assert.True(prestamo.Renovado);
    }

    [Fact]
    public void Renovar_SegundaVezNoSePermite()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);
        prestamo.Renovar(Estudiante(), false, Hoy.AddDays(3));

        Assert.Throws<RenovacionNoPermitidaException>(() => prestamo.Renovar(Estudiante(), false, Hoy.AddDays(5)));
    }

    [Fact]
    public void Renovar_NoSePermiteSiElMaterialEstaReservado()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);

        Assert.Throws<RenovacionNoPermitidaException>(() => prestamo.Renovar(Estudiante(), materialReservado: true, Hoy.AddDays(3)));
    }

    [Fact]
    public void Renovar_NoSePermiteSiYaFueDevuelto()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);
        prestamo.Devolver(Hoy.AddDays(1));

        Assert.Throws<RenovacionNoPermitidaException>(() => prestamo.Renovar(Estudiante(), false, Hoy.AddDays(2)));
    }

    [Fact]
    public void Renovar_NoSePermiteSiYaEstaVencido()
    {
        var prestamo = Prestamo.Crear(10, 20, Estudiante(), Hoy);

        Assert.Throws<RenovacionNoPermitidaException>(() => prestamo.Renovar(Estudiante(), false, Hoy.AddDays(8)));
    }

    [Fact]
    public void Prestamo_RechazaVencimientoAntesDelPrestamo()
    {
        Assert.Throws<DatoInvalidoException>(() => new Prestamo(1, 10, 20, Hoy, new DateOnly(2026, 11, 19), null, false));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 50)]
    [InlineData(8, 400)]
    [InlineData(40, 2000)]
    [InlineData(100, 2000)]
    public void Multa_CalculaCincuentaColonesPorDiaConTopeDeDosMil(int dias, decimal esperado)
    {
        Assert.Equal(esperado, Multa.CalcularMonto(dias));
    }

    [Fact]
    public void Multa_RechazaDiasNegativos()
    {
        Assert.Throws<DatoInvalidoException>(() => Multa.CalcularMonto(-1));
    }

    [Fact]
    public void Multa_GenerarDesdeUnPrestamoAtrasado()
    {
        var prestamo = new Prestamo(7, 10, 20, Hoy, new DateOnly(2026, 11, 27), null, false);

        var multa = Multa.Generar(prestamo, new DateTime(2026, 12, 5, 8, 0, 0));

        Assert.NotNull(multa);
        Assert.Equal(7, multa.PrestamoId);
        Assert.Equal(10, multa.UsuarioId);
        Assert.Equal(8, multa.DiasAtraso);
        Assert.Equal(400m, multa.Monto);
        Assert.Equal(EstadoMulta.Pendiente, multa.Estado);
    }

    [Fact]
    public void Multa_NoSeGeneraSiNoHayAtraso()
    {
        var prestamo = new Prestamo(7, 10, 20, Hoy, new DateOnly(2026, 11, 27), null, false);

        Assert.Null(Multa.Generar(prestamo, new DateTime(2026, 11, 27, 18, 0, 0)));
    }

    [Fact]
    public void Multa_Pagar_CambiaElEstadoYGuardaLaFecha()
    {
        var multa = new Multa(1, 7, 10, 3, 150m, EstadoMulta.Pendiente, Hoy, null);

        multa.Pagar(Hoy.AddDays(1));

        Assert.Equal(EstadoMulta.Pagada, multa.Estado);
        Assert.Equal(Hoy.AddDays(1), multa.FechaPago);
    }

    [Fact]
    public void Multa_PagarDosVecesNoSePermite()
    {
        var multa = new Multa(1, 7, 10, 3, 150m, EstadoMulta.Pagada, Hoy, Hoy);

        Assert.Throws<OperacionNoPermitidaException>(() => multa.Pagar(Hoy.AddDays(1)));
    }

    private static Rol Estudiante() => new(3, "Estudiante", limitePrestamos: 3, diasPrestamo: 7);
}
