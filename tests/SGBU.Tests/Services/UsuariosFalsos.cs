using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class UsuariosFalsos : IRepositorioUsuarios
{
    private readonly FabricaFalsa datos;

    public UsuariosFalsos(FabricaFalsa datos) => this.datos = datos;

    public Task<Usuario?> ObtenerPorIdAsync(int id) =>
        Task.FromResult(datos.Usuarios.FirstOrDefault(u => u.Id == id));

    public Task<Usuario?> ObtenerPorCarneAsync(string carne) =>
        Task.FromResult(datos.Usuarios.FirstOrDefault(u => string.Equals(u.Carne, carne, StringComparison.OrdinalIgnoreCase)));

    public Task<Usuario?> ObtenerPorEmailAsync(string email) =>
        Task.FromResult(datos.Usuarios.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<Pagina<Usuario>> BuscarAsync(string? texto, int numeroPagina, int tamanoPagina)
    {
        var filtrados = datos.Usuarios.Where(u => texto == null || u.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
        var pagina = filtrados.Skip((numeroPagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();
        return Task.FromResult(new Pagina<Usuario>(pagina, numeroPagina, tamanoPagina, filtrados.Count));
    }

    public Task<IReadOnlyList<Rol>> ListarRolesAsync() => Task.FromResult<IReadOnlyList<Rol>>(datos.Roles);

    public Task<int> AgregarAsync(Usuario usuario)
    {
        var id = datos.Usuarios.Count + 100;
        datos.Usuarios.Add(CopiarConId(usuario, id));
        return Task.FromResult(id);
    }

    public Task ActualizarAsync(Usuario usuario)
    {
        datos.Usuarios.RemoveAll(u => u.Id == usuario.Id);
        datos.Usuarios.Add(usuario);
        return Task.CompletedTask;
    }

    private static Usuario CopiarConId(Usuario u, int id)
    {
        var copia = new Usuario(u.Carne, u.Dni, u.Nombre, u.Apellidos, u.Email, u.Telefono, u.Rol, u.PasswordHash) { Id = id };
        if (!u.Activo)
        {
            copia.Desactivar();
        }

        return copia;
    }
}
