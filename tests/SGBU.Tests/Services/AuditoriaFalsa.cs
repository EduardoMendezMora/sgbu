using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class AuditoriaFalsa : IRepositorioAuditoria
{
    private readonly List<RegistroAuditoria> pendientes;
    private readonly List<RegistroAuditoria> guardados;

    public AuditoriaFalsa(List<RegistroAuditoria> pendientes, List<RegistroAuditoria> guardados)
    {
        this.pendientes = pendientes;
        this.guardados = guardados;
    }

    public Task<Pagina<AuditoriaDetalle>> BuscarAsync(string? texto, DateTime desde, DateTime hasta, int numeroPagina, int tamanoPagina)
    {
        var filtrados = guardados
            .Where(r => r.Fecha >= desde && r.Fecha < hasta.Date.AddDays(1))
            .Where(r => texto == null || r.Accion.Contains(texto, StringComparison.OrdinalIgnoreCase))
            .Select(r => new AuditoriaDetalle { Fecha = r.Fecha, Accion = r.Accion, Usuario = "Usuario " + r.UsuarioId })
            .ToList();
        var pagina = filtrados.Skip((numeroPagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();
        return Task.FromResult(new Pagina<AuditoriaDetalle>(pagina, numeroPagina, tamanoPagina, filtrados.Count));
    }

    public Task RegistrarAsync(RegistroAuditoria registro)
    {
        pendientes.Add(registro);
        return Task.CompletedTask;
    }
}
