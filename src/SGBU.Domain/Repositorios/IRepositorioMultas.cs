using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioMultas
{
    Task<bool> TieneMultasPendientesAsync(int usuarioId);

    Task<IReadOnlyList<Multa>> ListarPendientesDeUsuarioAsync(int usuarioId);

    Task<int> AgregarAsync(Multa multa);

    Task ActualizarAsync(Multa multa);
}
