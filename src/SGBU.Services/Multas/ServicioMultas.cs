using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Seguridad;

namespace SGBU.Services.Multas;

// Casos de uso de multas: verlas y registrar su pago.
// Las multas se crean solas al devolver un préstamo tarde (sp_RegistrarDevolucion).
public class ServicioMultas
{
    private readonly IFabricaUnidadDeTrabajo fabrica;

    public ServicioMultas(IFabricaUnidadDeTrabajo fabrica)
    {
        this.fabrica = fabrica;
    }

    public static decimal Total(IReadOnlyList<MultaDetalle> multas)
    {
        decimal total = 0;
        foreach (MultaDetalle multa in multas)
        {
            total += multa.Monto;
        }

        return total;
    }

    public async Task<IReadOnlyList<MultaDetalle>> ListarPendientesAsync(Sesion sesion, string? texto)
    {
        RevisarPermiso(sesion);
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Multas.ListarPendientesDetalleAsync(texto, null);
    }

    public async Task<IReadOnlyList<MultaDetalle>> ListarMisMultasAsync(Sesion sesion)
    {
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Multas.ListarPendientesDetalleAsync(null, sesion.UsuarioId);
    }

    public async Task PagarAsync(Sesion sesion, int multaId)
    {
        RevisarPermiso(sesion);
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        Multa? multa = await uow.Multas.ObtenerPorIdAsync(multaId);
        if (multa == null)
        {
            throw new DatoInvalidoException("La multa no existe.");
        }

        if (multa.Estado == EstadoMulta.Pagada)
        {
            throw new OperacionNoPermitidaException("La multa ya está pagada.");
        }

        await uow.Multas.PagarAsync(multaId, sesion.UsuarioId);
        await uow.ConfirmarAsync();
    }

    private static void RevisarPermiso(Sesion sesion)
    {
        if (!sesion.Puede(Permiso.GestionarMultas))
        {
            throw new OperacionNoPermitidaException("No tiene permiso para gestionar multas.");
        }
    }
}
