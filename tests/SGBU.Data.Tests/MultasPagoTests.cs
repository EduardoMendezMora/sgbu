using SGBU.Domain.Excepciones;
using Xunit;

namespace SGBU.Data.Tests;

public class MultasPagoTests
{
    [FactConBase]
    public async Task ListarPendientesDetalle_TraeLaMultaDelSeedConElTitulo()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var multas = await uow.Multas.ListarPendientesDetalleAsync("María", null);

        var multa = Assert.Single(multas);
        Assert.Equal("EST-1002", multa.Carne);
        Assert.Equal("Clean Code", multa.Titulo);
        Assert.Equal(400m, multa.Monto);
    }

    [FactConBase]
    public async Task Pagar_DesbloqueaYNoPermiteSegundoPago()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var maria = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1002"))!;
        var multa = (await uow.Multas.ListarPendientesDetalleAsync(null, maria.Id))[0];

        await uow.Multas.PagarAsync(multa.Id, maria.Id);

        Assert.False(await uow.Multas.TieneMultasPendientesAsync(maria.Id));
        Assert.NotNull((await uow.Multas.ObtenerPorIdAsync(multa.Id))!.FechaPago);
        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => uow.Multas.PagarAsync(multa.Id, maria.Id));
    }
}
