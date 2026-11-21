using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

/// <summary>
/// Implementaciones falsas, en memoria, de las interfaces de Domain. Gracias a que Services
/// solo conoce interfaces, se puede probar el login sin SQL Server: estas clases hacen de base.
/// </summary>
internal class FabricaFalsa : IFabricaUnidadDeTrabajo
{
    public List<Usuario> Usuarios { get; } = [];

    public List<RegistroAuditoria> Auditoria { get; } = [];

    public int Confirmaciones { get; private set; }

    public Task<IUnidadDeTrabajo> CrearAsync() =>
        Task.FromResult<IUnidadDeTrabajo>(new UnidadFalsa(this));

    private sealed class UnidadFalsa(FabricaFalsa fabrica) : IUnidadDeTrabajo
    {
        private readonly List<RegistroAuditoria> pendientes = [];

        public IRepositorioMateriales Materiales => throw new NotSupportedException();

        public IRepositorioUsuarios Usuarios { get; } = new UsuariosFalsos(fabrica.Usuarios);

        public IRepositorioPrestamos Prestamos => throw new NotSupportedException();

        public IRepositorioMultas Multas => throw new NotSupportedException();

        public IRepositorioReservas Reservas => throw new NotSupportedException();

        public IRepositorioAuditoria Auditoria => new AuditoriaFalsa(pendientes);

        public Task ConfirmarAsync()
        {
            fabrica.Auditoria.AddRange(pendientes);
            fabrica.Confirmaciones++;
            return Task.CompletedTask;
        }

        public Task RevertirAsync()
        {
            pendientes.Clear();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class AuditoriaFalsa(List<RegistroAuditoria> destino) : IRepositorioAuditoria
    {
        public Task RegistrarAsync(RegistroAuditoria registro)
        {
            destino.Add(registro);
            return Task.CompletedTask;
        }
    }

    private sealed class UsuariosFalsos(List<Usuario> datos) : IRepositorioUsuarios
    {
        public Task<Usuario?> ObtenerPorIdAsync(int id) =>
            Task.FromResult(datos.FirstOrDefault(u => u.Id == id));

        public Task<Usuario?> ObtenerPorCarneAsync(string carne) =>
            Task.FromResult(datos.FirstOrDefault(u => string.Equals(u.Carne, carne, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> ObtenerPorEmailAsync(string email) =>
            Task.FromResult(datos.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<Pagina<Usuario>> BuscarAsync(string? texto, int numeroPagina, int tamanoPagina) =>
            throw new NotSupportedException();

        public Task<int> AgregarAsync(Usuario usuario) => throw new NotSupportedException();

        public Task ActualizarAsync(Usuario usuario) => throw new NotSupportedException();
    }
}
