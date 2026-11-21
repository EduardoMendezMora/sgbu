using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioMultas
{
    Task<bool> TieneMultasPendientesAsync(int usuarioId, CancellationToken ct = default);

    Task<IReadOnlyList<Multa>> ListarPendientesDeUsuarioAsync(int usuarioId, CancellationToken ct = default);

    Task<int> AgregarAsync(Multa multa, CancellationToken ct = default);

    Task ActualizarAsync(Multa multa, CancellationToken ct = default);
}
