using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using Xunit;

namespace SGBU.Data.Tests;

public class RepositorioUsuariosTests
{
    private const string Hash = "$2b$11$vjXiYk5DCKFhMyyikFj60.hwFToLc0LKwceoGPBPZZyC8F7QIm4Vy";

    [FactConBase]
    public async Task ObtenerPorCarne_TraeElUsuarioConSuRol()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var usuario = await uow.Usuarios.ObtenerPorCarneAsync("EST-1001");

        Assert.NotNull(usuario);
        Assert.Equal("Carlos Jiménez Araya", usuario.NombreCompleto);
        Assert.Equal("Estudiante", usuario.Rol.Nombre);
        Assert.Equal(3, usuario.Rol.LimitePrestamos);
        Assert.True(usuario.Activo);
        Assert.Equal(60, usuario.PasswordHash.Length);
    }

    [FactConBase]
    public async Task ObtenerPorEmail_SirveParaElLogin()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var usuario = await uow.Usuarios.ObtenerPorEmailAsync("admin@sgbu.ac.cr");

        Assert.Equal("Administrador", usuario!.Rol.Nombre);
        Assert.Null(await uow.Usuarios.ObtenerPorEmailAsync("nadie@sgbu.ac.cr"));
    }

    [FactConBase]
    public async Task Buscar_PorNombreConPaginacion()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var pagina = await uow.Usuarios.BuscarAsync("EST-", 1, 2);

        Assert.Equal(2, pagina.Elementos.Count);
        Assert.Equal(5, pagina.Total);
    }

    [FactConBase]
    public async Task Agregar_Desactivar_YLeer()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var rol = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1001"))!.Rol;
        var nuevo = new Usuario("EST-9999", "109990999", "Prueba", "Integración", "prueba@x.ac.cr", null, rol, Hash);

        var id = await uow.Usuarios.AgregarAsync(nuevo);
        var leido = (await uow.Usuarios.ObtenerPorIdAsync(id))!;
        leido.Desactivar();
        await uow.Usuarios.ActualizarAsync(leido);

        Assert.False((await uow.Usuarios.ObtenerPorIdAsync(id))!.Activo);
    }

    [FactConBase]
    public async Task Agregar_EmailRepetido_LanzaExcepcionDeDominio()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var rol = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1001"))!.Rol;
        var repetido = new Usuario("EST-9998", "109980998", "X", "Y", "admin@sgbu.ac.cr", null, rol, Hash);

        var ex = await Assert.ThrowsAsync<DatoInvalidoException>(() => uow.Usuarios.AgregarAsync(repetido));
        Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
