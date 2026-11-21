using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Services.Prestamos;
using SGBU.Services.Seguridad;
using Xunit;

namespace SGBU.Tests.Services;

public class ServicioPrestamosTests
{
    private const string Hash = "$2b$11$vjXiYk5DCKFhMyyikFj60.hwFToLc0LKwceoGPBPZZyC8F7QIm4Vy";
    private static readonly Sesion Admin = new(1, "Ana Rojas", "Administrador", DateTime.Now);
    private static readonly Sesion Carlos = new(4, "Carlos Jiménez", "Estudiante", DateTime.Now);
    private static readonly Sesion Maria = new(5, "María Vargas", "Estudiante", DateTime.Now);

    [Fact]
    public async Task Registrar_ConCarneEIsbn_LlamaAlProcedimientoYConfirma()
    {
        var (servicio, fabrica) = Crear();

        var prestamo = await servicio.RegistrarAsync(Admin, "EST-1001", "978-0-13-235088-4");

        Assert.Equal(4, prestamo.UsuarioId);
        Assert.Equal(["prestar 1"], fabrica.LlamadasProcedimientos);
        Assert.Equal(1, fabrica.Confirmaciones);
    }

    [Fact]
    public async Task Registrar_UsuarioConMultas_NoPresta()
    {
        var (servicio, fabrica) = Crear();
        fabrica.UsuariosConMulta.Add(4);

        await Assert.ThrowsAsync<MultasPendientesException>(() => servicio.RegistrarAsync(Admin, "EST-1001", "9780132350884"));
        Assert.Empty(fabrica.LlamadasProcedimientos);
    }

    [Fact]
    public async Task Registrar_LlegoAlLimiteDelRol_NoPresta()
    {
        var (servicio, fabrica) = Crear();
        for (var i = 0; i < 3; i++)
        {
            fabrica.Prestamos.Add(new Prestamo(50 + i, 4, 2, DateTime.Now, DateOnly.FromDateTime(DateTime.Now).AddDays(7), null, false));
        }

        await Assert.ThrowsAsync<LimitePrestamosException>(() => servicio.RegistrarAsync(Admin, "EST-1001", "9780132350884"));
    }

    [Fact]
    public async Task Registrar_SinStock_NoPresta()
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<StockInsuficienteException>(() => servicio.RegistrarAsync(Admin, "EST-1001", "9789876543217"));
    }

    [Theory]
    [InlineData("EST-9999", "9780132350884")]
    [InlineData("EST-1001", "9780000000002")]
    [InlineData("", "9780132350884")]
    public async Task Registrar_UsuarioOMaterialQueNoExiste_DaDatoInvalido(string carne, string isbn)
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<DatoInvalidoException>(() => servicio.RegistrarAsync(Admin, carne, isbn));
    }

    [Fact]
    public async Task Registrar_EstudianteNoPuedeRegistrarPrestamos()
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.RegistrarAsync(Carlos, "EST-1001", "9780132350884"));
    }

    [Fact]
    public async Task Devolver_DevuelveLosDiasYLaMulta()
    {
        var (servicio, fabrica) = Crear();

        var resultado = await servicio.DevolverAsync(Admin, 7);

        Assert.Equal(3, resultado.DiasAtraso);
        Assert.Equal(150m, resultado.Monto);
        Assert.Equal(["devolver 7"], fabrica.LlamadasProcedimientos);
        Assert.Equal(1, fabrica.Confirmaciones);
    }

    [Fact]
    public async Task Renovar_ElDuenoPuedeRenovarSuPrestamo()
    {
        var (servicio, fabrica) = Crear();
        fabrica.Prestamos.Add(PrestamoDeCarlos(10));

        await servicio.RenovarAsync(Carlos, 10);

        Assert.Equal(["renovar 10"], fabrica.LlamadasProcedimientos);
    }

    [Fact]
    public async Task Renovar_OtroEstudianteNoPuede()
    {
        var (servicio, fabrica) = Crear();
        fabrica.Prestamos.Add(PrestamoDeCarlos(10));

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.RenovarAsync(Maria, 10));
    }

    [Fact]
    public async Task Renovar_MaterialReservado_NoSePermite()
    {
        var (servicio, fabrica) = Crear();
        fabrica.Prestamos.Add(PrestamoDeCarlos(10));
        fabrica.MaterialesReservados.Add(1);

        await Assert.ThrowsAsync<RenovacionNoPermitidaException>(() => servicio.RenovarAsync(Admin, 10));
        Assert.Empty(fabrica.LlamadasProcedimientos);
    }

    [Fact]
    public async Task ListarMisPrestamos_SoloTraeLosDelUsuario()
    {
        var (servicio, fabrica) = Crear();
        fabrica.Prestamos.Add(PrestamoDeCarlos(10));
        fabrica.Prestamos.Add(new Prestamo(11, 5, 1, DateTime.Now, DateOnly.FromDateTime(DateTime.Now).AddDays(7), null, false));

        var mios = await servicio.ListarMisPrestamosAsync(Carlos);
        var todos = await servicio.ListarActivosAsync(Admin, null);

        Assert.Equal(10, Assert.Single(mios).Id);
        Assert.Equal(2, todos.Count);
        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.ListarActivosAsync(Carlos, null));
    }

    private static Prestamo PrestamoDeCarlos(int id) =>
        new(id, 4, 1, DateTime.Now, DateOnly.FromDateTime(DateTime.Now).AddDays(5), null, false);

    private static (ServicioPrestamos Servicio, FabricaFalsa Fabrica) Crear()
    {
        var fabrica = new FabricaFalsa();
        var estudiante = new Rol(3, "Estudiante", 3, 7);
        fabrica.Usuarios.Add(new Usuario("EST-1001", "104440444", "Carlos", "Jiménez", "carlos@estudiante.ac.cr", null, estudiante, Hash) { Id = 4 });
        fabrica.Usuarios.Add(new Usuario("EST-1002", "105550555", "María", "Vargas", "maria@estudiante.ac.cr", null, estudiante, Hash) { Id = 5 });
        fabrica.Materiales.Add(new Libro(1, "9780132350884", "Clean Code", "Robert C. Martin", 5, 4, null));
        fabrica.Materiales.Add(new Libro(2, "9789876543217", "Estructuras de datos en C#", "Joyanes", 1, 0, null));
        return (new ServicioPrestamos(fabrica), fabrica);
    }
}
