using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioUsuarios
{
    Task<Usuario?> ObtenerPorIdAsync(int id);

    Task<Usuario?> ObtenerPorCarneAsync(string carne);

    /// <summary>Para el login.</summary>
    Task<Usuario?> ObtenerPorEmailAsync(string email);

    Task<Pagina<Usuario>> BuscarAsync(string? texto, int numeroPagina, int tamanoPagina);

    Task<IReadOnlyList<Rol>> ListarRolesAsync();

    Task<int> AgregarAsync(Usuario usuario);

    Task ActualizarAsync(Usuario usuario);
}
