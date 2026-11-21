using SGBU.Domain.Entidades;
using Xunit;

namespace SGBU.Data.Tests;

public class RepositorioMultasReservasTests
{
    [FactConBase]
    public async Task Multas_DetectaPendientesDelSeed()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var conMulta = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1002"))!;
        var sinMulta = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1001"))!;

        Assert.True(await uow.Multas.TieneMultasPendientesAsync(conMulta.Id));
        Assert.False(await uow.Multas.TieneMultasPendientesAsync(sinMulta.Id));

        var multa = Assert.Single(await uow.Multas.ListarPendientesDeUsuarioAsync(conMulta.Id));
        Assert.Equal(400m, multa.Monto);
        Assert.Equal(8, multa.DiasAtraso);
    }

    [FactConBase]
    public async Task Multas_PagarLaUnicaPendiente_DesbloqueaAlUsuario()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var usuario = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1002"))!;
        var multa = (await uow.Multas.ListarPendientesDeUsuarioAsync(usuario.Id))[0];

        multa.Pagar(DateTime.Now);
        await uow.Multas.ActualizarAsync(multa);

        Assert.False(await uow.Multas.TieneMultasPendientesAsync(usuario.Id));
    }

    [FactConBase]
    public async Task Multas_AgregarDesdeUnPrestamoAtrasado()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var usuario = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1004"))!;
        var prestamo = (await uow.Prestamos.ListarActivosDeUsuarioAsync(usuario.Id))[0];
        var hoy = DateTime.Now;

        prestamo.Devolver(hoy);
        await uow.Prestamos.ActualizarAsync(prestamo);
        var multa = Multa.Generar(prestamo, hoy)!;
        var id = await uow.Multas.AgregarAsync(multa);

        Assert.True(id > 0);
        Assert.True(await uow.Multas.TieneMultasPendientesAsync(usuario.Id));
    }

    [FactConBase]
    public async Task Reservas_ExistePendienteYSePuedeCancelar()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var reservado = (await uow.Materiales.ObtenerPorIsbnAsync("9786073212342"))!;
        var libre = (await uow.Materiales.ObtenerPorIsbnAsync("9780134685991"))!;
        var usuario = (await uow.Usuarios.ObtenerPorCarneAsync("EST-1003"))!;

        Assert.True(await uow.Reservas.ExistePendienteAsync(reservado.Id));
        Assert.False(await uow.Reservas.ExistePendienteAsync(libre.Id));

        var reserva = Reserva.Crear(usuario.Id, libre.Id, DateTime.Now);
        await uow.Reservas.AgregarAsync(reserva);
        Assert.True(await uow.Reservas.ExistePendienteAsync(libre.Id));
    }
}
