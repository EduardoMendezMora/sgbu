using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using Xunit;

namespace SGBU.Tests.Domain;

public class UsuarioTests
{
    private const string Hash = "$2b$11$vjXiYk5DCKFhMyyikFj60.hwFToLc0LKwceoGPBPZZyC8F7QIm4Vy";

    [Fact]
    public void Usuario_SeCreaActivoConSusDatos()
    {
        var usuario = CrearEstudiante();

        Assert.True(usuario.Activo);
        Assert.Equal("Carlos Jiménez Araya", usuario.NombreCompleto);
        Assert.Equal("Estudiante", usuario.Rol.Nombre);
    }

    [Theory]
    [InlineData("correo-malo", "104440444", "7000-1001")]
    [InlineData("carlos@estudiante.ac.cr", "123", "7000-1001")]
    [InlineData("carlos@estudiante.ac.cr", "104440444", "12")]
    public void Usuario_RechazaEmailDniOTelefonoInvalidos(string email, string dni, string telefono)
    {
        Assert.Throws<DatoInvalidoException>(() =>
            new Usuario("EST-1001", dni, "Carlos", "Jiménez", email, telefono, Estudiante(), Hash));
    }

    [Fact]
    public void Usuario_TelefonoEsOpcional()
    {
        var usuario = new Usuario("EST-1001", "104440444", "Carlos", "Jiménez", "c@x.cr", null, Estudiante(), Hash);

        Assert.Null(usuario.Telefono);
    }

    [Fact]
    public void Usuario_RechazaHashQueNoEsBCrypt()
    {
        Assert.Throws<DatoInvalidoException>(() =>
            new Usuario("EST-1001", "104440444", "Carlos", "Jiménez", "c@x.cr", null, Estudiante(), "clave-en-texto-plano"));
    }

    [Fact]
    public void ValidarPuedePrestar_PermiteSiEstaBajoElLimiteYSinMultas()
    {
        var usuario = CrearEstudiante();

        var excepcion = Record.Exception(() => usuario.ValidarPuedePrestar(prestamosActivos: 2, tieneMultasPendientes: false));

        Assert.Null(excepcion);
    }

    [Fact]
    public void ValidarPuedePrestar_RechazaSiLlegoAlLimiteDelRol()
    {
        var usuario = CrearEstudiante();

        var ex = Assert.Throws<LimitePrestamosException>(() => usuario.ValidarPuedePrestar(3, false));
        Assert.Contains("3", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidarPuedePrestar_RechazaSiTieneMultasPendientes()
    {
        var usuario = CrearEstudiante();

        Assert.Throws<MultasPendientesException>(() => usuario.ValidarPuedePrestar(0, tieneMultasPendientes: true));
    }

    [Fact]
    public void ValidarPuedePrestar_RechazaUsuarioInactivo()
    {
        var usuario = CrearEstudiante();
        usuario.Desactivar();

        Assert.Throws<OperacionNoPermitidaException>(() => usuario.ValidarPuedePrestar(0, false));
    }

    [Fact]
    public void Rol_RechazaLimitesNoPositivos()
    {
        Assert.Throws<DatoInvalidoException>(() => new Rol(1, "Estudiante", 0, 7));
        Assert.Throws<DatoInvalidoException>(() => new Rol(1, "Estudiante", 3, 0));
    }

    [Fact]
    public void Excepciones_DeNegocioHeredanDeExcepcionDominio()
    {
        Assert.IsAssignableFrom<ExcepcionDominio>(new StockInsuficienteException("Clean Code"));
        Assert.IsAssignableFrom<ExcepcionDominio>(new MultasPendientesException());
        Assert.IsAssignableFrom<ExcepcionDominio>(new LimitePrestamosException("Estudiante", 3));
    }

    [Fact]
    public void Reserva_SeCreaPendienteYSePuedeCancelar()
    {
        var reserva = Reserva.Crear(usuarioId: 10, materialId: 20, new DateTime(2026, 11, 20));

        Assert.Equal(EstadoReserva.Pendiente, reserva.Estado);

        reserva.Cancelar();

        Assert.Equal(EstadoReserva.Cancelada, reserva.Estado);
        Assert.Throws<OperacionNoPermitidaException>(() => reserva.Atender());
    }

    [Fact]
    public void RegistroAuditoria_ExigeAccion()
    {
        Assert.Throws<DatoInvalidoException>(() => new RegistroAuditoria(1, " ", "Prestamos", 5, null, DateTime.Now));

        var registro = new RegistroAuditoria(1, "PRESTAMO", "Prestamos", 5, "ISBN 9780132350884", new DateTime(2026, 11, 20));
        Assert.Equal("PRESTAMO", registro.Accion);
    }

    private static Rol Estudiante() => new(3, "Estudiante", 3, 7);

    private static Usuario CrearEstudiante() =>
        new("EST-1001", "104440444", "Carlos", "Jiménez Araya", "carlos@estudiante.ac.cr", "7000-1001", Estudiante(), Hash);
}
