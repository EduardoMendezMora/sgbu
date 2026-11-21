using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Services.Seguridad;
using Xunit;

namespace SGBU.Tests.Services;

public class ServicioAutenticacionTests
{
    private const string Clave = "Sgbu2026!";

    // Hash BCrypt de "Sgbu2026!" (el mismo del seed de la base).
    private const string Hash = "$2b$11$vjXiYk5DCKFhMyyikFj60.hwFToLc0LKwceoGPBPZZyC8F7QIm4Vy";

    private static readonly DateTime Ahora = new(2026, 11, 21, 8, 30, 0);

    [Fact]
    public async Task IniciarSesion_ConEmailYClaveCorrectos_DevuelveLaSesion()
    {
        var (servicio, fabrica) = Crear();

        var sesion = await servicio.IniciarSesionAsync("carlos@estudiante.ac.cr", Clave);

        Assert.Equal(4, sesion.UsuarioId);
        Assert.Equal("Carlos Jiménez Araya", sesion.NombreCompleto);
        Assert.Equal("Estudiante", sesion.Rol);
        Assert.Equal(1, fabrica.Confirmaciones);
    }

    [Fact]
    public async Task IniciarSesion_TambienAceptaElCarne()
    {
        var (servicio, _) = Crear();

        var sesion = await servicio.IniciarSesionAsync("  est-1001 ", Clave);

        Assert.Equal(4, sesion.UsuarioId);
    }

    [Fact]
    public async Task IniciarSesion_RegistraElLoginEnAuditoria()
    {
        var (servicio, fabrica) = Crear();

        await servicio.IniciarSesionAsync("carlos@estudiante.ac.cr", Clave);

        var registro = Assert.Single(fabrica.Auditoria);
        Assert.Equal("LOGIN", registro.Accion);
        Assert.Equal(4, registro.UsuarioId);
    }

    [Fact]
    public async Task IniciarSesion_ClaveIncorrecta_LanzaCredencialesInvalidasYAudita()
    {
        var (servicio, fabrica) = Crear();

        await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            servicio.IniciarSesionAsync("carlos@estudiante.ac.cr", "otra-clave"));

        var registro = Assert.Single(fabrica.Auditoria);
        Assert.Equal("LOGIN_FALLIDO", registro.Accion);
        Assert.Equal(4, registro.UsuarioId);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioQueNoExiste_DaElMismoErrorQueClaveIncorrecta()
    {
        var (servicio, fabrica) = Crear();

        var noExiste = await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            servicio.IniciarSesionAsync("nadie@x.cr", Clave));
        var claveMala = await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            servicio.IniciarSesionAsync("carlos@estudiante.ac.cr", "mala"));

        // Mismo mensaje: así nadie puede averiguar qué correos están registrados.
        Assert.Equal(claveMala.Message, noExiste.Message);
        Assert.Null(fabrica.Auditoria[0].UsuarioId);
        Assert.Contains("nadie@x.cr", fabrica.Auditoria[0].Detalle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioInactivo_NoEntra()
    {
        var (servicio, fabrica) = Crear();
        fabrica.Usuarios[0].Desactivar();

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() =>
            servicio.IniciarSesionAsync("carlos@estudiante.ac.cr", Clave));
    }

    [Theory]
    [InlineData("", Clave)]
    [InlineData("carlos@estudiante.ac.cr", "")]
    [InlineData("   ", "  ")]
    public async Task IniciarSesion_DatosVacios_PideCompletarlos(string usuario, string clave)
    {
        var (servicio, fabrica) = Crear();

        await Assert.ThrowsAsync<DatoInvalidoException>(() => servicio.IniciarSesionAsync(usuario, clave));
        Assert.Empty(fabrica.Auditoria);
    }

    [Fact]
    public void HasherBCrypt_GeneraHashQueSeVerificaYNoEsLaClave()
    {
        var hasher = new HasherBCrypt();

        var hash = hasher.Generar(Clave);

        Assert.Equal(60, hash.Length);
        Assert.NotEqual(Clave, hash);
        Assert.True(hasher.Verificar(Clave, hash));
        Assert.False(hasher.Verificar("otra", hash));
        Assert.True(hasher.Verificar(Clave, Hash));
    }

    [Fact]
    public void HasherBCrypt_DosHashesDeLaMismaClaveSonDistintos()
    {
        var hasher = new HasherBCrypt();

        // Cada hash lleva su propia "sal" aleatoria.
        Assert.NotEqual(hasher.Generar(Clave), hasher.Generar(Clave));
    }

    [Theory]
    [InlineData("Administrador", Permiso.GestionarUsuarios, true)]
    [InlineData("Administrador", Permiso.VerAuditoria, true)]
    [InlineData("Profesor", Permiso.VerCatalogo, true)]
    [InlineData("Profesor", Permiso.VerReportes, true)]
    [InlineData("Profesor", Permiso.GestionarUsuarios, false)]
    [InlineData("Estudiante", Permiso.VerCatalogo, true)]
    [InlineData("Estudiante", Permiso.VerMisPrestamos, true)]
    [InlineData("Estudiante", Permiso.RegistrarPrestamos, false)]
    [InlineData("Estudiante", Permiso.VerReportes, false)]
    public void Sesion_PermisosSegunElRol(string rol, Permiso permiso, bool esperado)
    {
        var sesion = new Sesion(1, "X", rol, Ahora);

        Assert.Equal(esperado, sesion.Puede(permiso));
    }

    [Fact]
    public void Sesion_RolDesconocido_NoTienePermisos()
    {
        var sesion = new Sesion(1, "X", "Visitante", Ahora);

        Assert.All(Enum.GetValues<Permiso>(), p => Assert.False(sesion.Puede(p)));
    }

    private static (ServicioAutenticacion Servicio, FabricaFalsa Fabrica) Crear()
    {
        var fabrica = new FabricaFalsa();
        fabrica.Usuarios.Add(new Usuario(
            "EST-1001",
            "104440444",
            "Carlos",
            "Jiménez Araya",
            "carlos@estudiante.ac.cr",
            null,
            new Rol(3, "Estudiante", 3, 7),
            Hash) { Id = 4 });

        return (new ServicioAutenticacion(fabrica, new HasherBCrypt()), fabrica);
    }
}
