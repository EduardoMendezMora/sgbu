using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioPrestamos
{
    Task<Prestamo?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    Task<int> ContarActivosDeUsuarioAsync(int usuarioId, CancellationToken ct = default);

    Task<IReadOnlyList<Prestamo>> ListarActivosDeUsuarioAsync(int usuarioId, CancellationToken ct = default);

    Task<int> AgregarAsync(Prestamo prestamo, CancellationToken ct = default);

    Task ActualizarAsync(Prestamo prestamo, CancellationToken ct = default);
}
