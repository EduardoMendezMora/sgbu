using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioPrestamos
{
    Task<Prestamo?> ObtenerPorIdAsync(int id);

    Task<int> ContarActivosDeUsuarioAsync(int usuarioId);

    Task<IReadOnlyList<Prestamo>> ListarActivosDeUsuarioAsync(int usuarioId);

    // Préstamos sin devolver con datos de usuario y material. Si usuarioId es null, trae todos.
    Task<IReadOnlyList<PrestamoDetalle>> ListarActivosDetalleAsync(string? texto, int? usuarioId);

    Task<int> AgregarAsync(Prestamo prestamo);

    Task ActualizarAsync(Prestamo prestamo);

    // Estos tres llaman a los stored procedures de 006_procedimientos.sql.
    Task<int> RegistrarAsync(int usuarioId, int materialId, int registradoPorId);

    Task<ResultadoDevolucion> DevolverAsync(int prestamoId, int registradoPorId);

    Task RenovarAsync(int prestamoId, int registradoPorId);
}
