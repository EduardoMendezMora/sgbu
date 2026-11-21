using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioUsuarios
{
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    Task<Usuario?> ObtenerPorCarneAsync(string carne, CancellationToken ct = default);

    /// <summary>Para el login.</summary>
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default);

    Task<Pagina<Usuario>> BuscarAsync(string? texto, int numeroPagina, int tamanoPagina, CancellationToken ct = default);

    Task<int> AgregarAsync(Usuario usuario, CancellationToken ct = default);

    Task ActualizarAsync(Usuario usuario, CancellationToken ct = default);
}
