using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioMultas
{
    Task<Multa?> ObtenerPorIdAsync(int id);

    Task<bool> TieneMultasPendientesAsync(int usuarioId);

    Task<IReadOnlyList<Multa>> ListarPendientesDeUsuarioAsync(int usuarioId);

    // Multas pendientes con usuario y material. Si usuarioId es null, trae las de todos.
    Task<IReadOnlyList<MultaDetalle>> ListarPendientesDetalleAsync(string? texto, int? usuarioId);

    Task<int> AgregarAsync(Multa multa);

    Task ActualizarAsync(Multa multa);

    // Llama a sp_PagarMulta.
    Task PagarAsync(int multaId, int registradoPorId);
}
