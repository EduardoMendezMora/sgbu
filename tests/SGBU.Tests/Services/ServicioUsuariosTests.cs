using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Services.Seguridad;
using SGBU.Services.Usuarios;
using Xunit;

namespace SGBU.Tests.Services;

public class ServicioUsuariosTests
{
    private const string Hash = "$2b$11$vjXiYk5DCKFhMyyikFj60.hwFToLc0LKwceoGPBPZZyC8F7QIm4Vy";
    private static readonly Sesion Admin = new(1, "Ana Rojas", "Administrador", DateTime.Now);
    private static readonly Sesion Profesor = new(2, "Ricardo Salas", "Profesor", DateTime.Now);

    [Fact]
    public async Task Guardar_Nuevo_GuardaElHashYNoLaClave()
    {
        var (servicio, fabrica) = Crear();
        var datos = Nuevo();

        await servicio.GuardarAsync(Admin, datos);

        var creado = fabrica.Usuarios.Single(u => u.Carne == "EST-2001");
        Assert.NotEqual("Clave2026!", creado.PasswordHash);
        Assert.True(new HasherBCrypt().Verificar("Clave2026!", creado.PasswordHash));
        Assert.Equal("Estudiante", creado.Rol.Nombre);
        Assert.Equal("USUARIO_CREADO", Assert.Single(fabrica.Auditoria).Accion);
    }

    [Fact]
    public async Task Guardar_Nuevo_ExigeContrasenaDeAlMenos8()
    {
        var (servicio, _) = Crear();
        var datos = Nuevo();
        datos.Contrasena = "corta";

        var ex = await Assert.ThrowsAsync<DatoInvalidoException>(() => servicio.GuardarAsync(Admin, datos));
        Assert.Contains("8", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Guardar_EmailInvalido_LanzaDatoInvalido()
    {
        var (servicio, _) = Crear();
        var datos = Nuevo();
        datos.Email = "sin-arroba";

        await Assert.ThrowsAsync<DatoInvalidoException>(() => servicio.GuardarAsync(Admin, datos));
    }

    [Fact]
    public async Task Guardar_Edicion_SinContrasena_ConservaLaAnterior()
    {
        var (servicio, fabrica) = Crear();
        var datos = FormularioUsuario.Desde(fabrica.Usuarios[0]);
        datos.Nombre = "Carlos Andrés";

        await servicio.GuardarAsync(Admin, datos);

        var editado = fabrica.Usuarios.Single(u => u.Id == 4);
        Assert.Equal("Carlos Andrés", editado.Nombre);
        Assert.Equal(Hash, editado.PasswordHash);
        Assert.Equal(4, editado.Id);
    }

    [Fact]
    public async Task Guardar_Edicion_PuedeDesactivarYCambiarElRol()
    {
        var (servicio, fabrica) = Crear();
        var datos = FormularioUsuario.Desde(fabrica.Usuarios[0]);
        datos.Activo = false;
        datos.RolId = 2;

        await servicio.GuardarAsync(Admin, datos);

        var editado = fabrica.Usuarios.Single(u => u.Id == 4);
        Assert.False(editado.Activo);
        Assert.Equal("Profesor", editado.Rol.Nombre);
    }

    [Fact]
    public async Task Guardar_SoloElAdministradorPuede()
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.GuardarAsync(Profesor, Nuevo()));
        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.BuscarAsync(Profesor, null, 1));
    }

    [Fact]
    public async Task ListarRoles_YBuscar()
    {
        var (servicio, _) = Crear();

        var roles = await servicio.ListarRolesAsync();
        var pagina = await servicio.BuscarAsync(Admin, "carlos", 1);

        Assert.Equal(3, roles.Count);
        Assert.Single(pagina.Elementos);
    }

    private static FormularioUsuario Nuevo() => new()
    {
        Carne = "EST-2001", Dni = "111110111", Nombre = "Sofía", Apellidos = "Arias", Email = "sofia@estudiante.ac.cr",
        Telefono = "7000-2001", RolId = 3, Contrasena = "Clave2026!", Activo = true,
    };

    private static (ServicioUsuarios Servicio, FabricaFalsa Fabrica) Crear()
    {
        var fabrica = new FabricaFalsa();
        fabrica.Roles.Add(new Rol(1, "Administrador", 10, 30));
        fabrica.Roles.Add(new Rol(2, "Profesor", 5, 14));
        fabrica.Roles.Add(new Rol(3, "Estudiante", 3, 7));
        fabrica.Usuarios.Add(new Usuario("EST-1001", "104440444", "Carlos", "Jiménez", "carlos@estudiante.ac.cr", null, fabrica.Roles[2], Hash) { Id = 4 });
        return (new ServicioUsuarios(fabrica, new HasherBCrypt()), fabrica);
    }
}
