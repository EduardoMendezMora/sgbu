using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioPrestamos
{
    Task<Prestamo?> ObtenerPorIdAsync(int id);

    Task<int> ContarActivosDeUsuarioAsync(int usuarioId);

    Task<IReadOnlyList<Prestamo>> ListarActivosDeUsuarioAsync(int usuarioId);

    Task<int> AgregarAsync(Prestamo prestamo);

    Task ActualizarAsync(Prestamo prestamo);
}
