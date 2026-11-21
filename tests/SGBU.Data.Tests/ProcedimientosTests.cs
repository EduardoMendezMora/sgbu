using SGBU.Domain.Excepciones;
using Xunit;

namespace SGBU.Data.Tests;

// Los stored procedures llamados desde C#, dentro de la transacción del Unit of Work
// (al final se revierte, la base queda igual).
public class ProcedimientosTests
{
    [FactConBase]
    public async Task Registrar_BajaElStockYCalculaElVencimientoSegunElRol()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var profesora = (await uow.Usuarios.ObtenerPorCarneAsync("PRF-002"))!;
        var admin = (await uow.Usuarios.ObtenerPorCarneAsync("ADM-001"))!;
        var material = (await uow.Materiales.ObtenerPorIsbnAsync("9789580001119"))!;

        var id = await uow.Prestamos.RegistrarAsync(profesora.Id, material.Id, admin.Id);

        var prestamo = (await uow.Prestamos.ObtenerPorIdAsync(id))!;
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today).AddDays(14), prestamo.FechaVencimiento);
        Assert.Equal(material.StockDisponible - 1, (await uow.Materiales.ObtenerPorIdAsync(material.Id))!.StockDisponible);
    }

    [FactConBase]
    public async Task Registrar_SinStock_LanzaOperacionNoPermitidaConElMensajeDelProcedimiento()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var profesora = (await uow.Usuarios.ObtenerPorCarneAsync("PRF-002"))!;
        var agotado = (await uow.Materiales.ObtenerPorIsbnAsync("9789876543217"))!;

        var ex = await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => uow.Prestamos.RegistrarAsync(profesora.Id, agotado.Id, profesora.Id));
        Assert.Contains("ejemplares disponibles", ex.Message, StringComparison.Ordinal);
    }

    [FactConBase]
    public async Task Devolver_Atrasado_GeneraLaMultaYSubeElStock()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var daniela = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1004"))!;
        var prestamo = (await uow.Prestamos.ListarActivosDeUsuarioAsync(daniela.Id))[0];

        var resultado = await uow.Prestamos.DevolverAsync(prestamo.Id, daniela.Id);

        Assert.Equal(13, resultado.DiasAtraso);
        Assert.Equal(650m, resultado.Monto);
        Assert.True(await uow.Multas.TieneMultasPendientesAsync(daniela.Id));
        Assert.False((await uow.Prestamos.ObtenerPorIdAsync(prestamo.Id))!.EstaActivo);
    }

    [FactConBase]
    public async Task Renovar_ExtiendeElVencimientoYNoPermiteUnaSegundaVez()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var carlos = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1001"))!;
        var prestamo = (await uow.Prestamos.ListarActivosDeUsuarioAsync(carlos.Id))[0];

        await uow.Prestamos.RenovarAsync(prestamo.Id, carlos.Id);

        var renovado = (await uow.Prestamos.ObtenerPorIdAsync(prestamo.Id))!;
        Assert.True(renovado.Renovado);
        Assert.Equal(prestamo.FechaVencimiento.AddDays(7), renovado.FechaVencimiento);
        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => uow.Prestamos.RenovarAsync(prestamo.Id, carlos.Id));
    }

    [FactConBase]
    public async Task ListarActivosDetalle_TraeUsuarioYMaterial()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var todos = await uow.Prestamos.ListarActivosDetalleAsync(null, null);
        var buscados = await uow.Prestamos.ListarActivosDetalleAsync("Daniela", null);

        Assert.Equal(6, todos.Count); // el seed tiene 10 préstamos, 4 devueltos
        var detalle = Assert.Single(buscados);
        Assert.Equal("EST-1004", detalle.Carne);
        Assert.Equal(13, detalle.DiasAtraso);
        Assert.False(string.IsNullOrEmpty(detalle.Titulo));
    }
}
