using SGBU.Domain.Entidades;
using Xunit;

namespace SGBU.Data.Tests;

public class RepositorioPrestamosTests
{
    [FactConBase]
    public async Task ContarYListarActivos_DelUsuario()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var usuario = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1001"))!;

        var cantidad = await uow.Prestamos.ContarActivosDeUsuarioAsync(usuario.Id);
        var activos = await uow.Prestamos.ListarActivosDeUsuarioAsync(usuario.Id);

        Assert.Equal(1, cantidad);
        Assert.All(activos, p => Assert.True(p.EstaActivo));
        Assert.Equal(cantidad, activos.Count);
    }

    [FactConBase]
    public async Task Agregar_Renovar_Devolver_GuardaCadaCambio()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var usuario = (await uow.Usuarios.ObtenerPorCarneAsync("PRF-002"))!;
        var material = (await uow.Materiales.ObtenerPorIsbnAsync("9789580001119"))!;
        var ahora = DateTime.Now;

        var prestamo = Prestamo.Crear(usuario.Id, material.Id, usuario.Rol, ahora);
        var id = await uow.Prestamos.AgregarAsync(prestamo);
        var leido = (await uow.Prestamos.ObtenerPorIdAsync(id))!;

        Assert.Equal(DateOnly.FromDateTime(ahora).AddDays(14), leido.FechaVencimiento);
        Assert.False(leido.Renovado);

        leido.Renovar(usuario.Rol, materialReservado: false, ahora);
        await uow.Prestamos.ActualizarAsync(leido);
        leido.Devolver(ahora.AddDays(1));
        await uow.Prestamos.ActualizarAsync(leido);

        var final = (await uow.Prestamos.ObtenerPorIdAsync(id))!;
        Assert.True(final.Renovado);
        Assert.False(final.EstaActivo);
        Assert.Equal(DateOnly.FromDateTime(ahora).AddDays(28), final.FechaVencimiento);
    }
}
