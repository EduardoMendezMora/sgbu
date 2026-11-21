using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioAuditoria
{
    Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct = default);
}
