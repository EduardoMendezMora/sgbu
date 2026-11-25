using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Seguridad;

namespace SGBU.Services.Auditoria;

// Consulta de la bitácora: quién hizo qué y cuándo. Solo el administrador.
public class ServicioAuditoria
{
    public const int TamanoPagina = 20;

    private readonly IFabricaUnidadDeTrabajo fabrica;

    public ServicioAuditoria(IFabricaUnidadDeTrabajo fabrica)
    {
        this.fabrica = fabrica;
    }

    public async Task<Pagina<AuditoriaDetalle>> BuscarAsync(Sesion sesion, string? texto, DateTime desde, DateTime hasta, int numeroPagina)
    {
        if (!sesion.Puede(Permiso.VerAuditoria))
        {
            throw new OperacionNoPermitidaException("Solo el administrador puede ver la auditoría.");
        }

        if (desde.Date > hasta.Date)
        {
            throw new DatoInvalidoException("La fecha \"desde\" no puede ser posterior a \"hasta\".");
        }

        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Auditoria.BuscarAsync(texto, desde, hasta, numeroPagina, TamanoPagina);
    }
}
