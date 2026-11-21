using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class AuditoriaFalsa : IRepositorioAuditoria
{
    private readonly List<RegistroAuditoria> destino;

    public AuditoriaFalsa(List<RegistroAuditoria> destino) => this.destino = destino;

    public Task RegistrarAsync(RegistroAuditoria registro)
    {
        destino.Add(registro);
        return Task.CompletedTask;
    }
}
