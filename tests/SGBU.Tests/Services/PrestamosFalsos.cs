using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class PrestamosFalsos : IRepositorioPrestamos
{
    private readonly FabricaFalsa datos;

    public PrestamosFalsos(FabricaFalsa datos) => this.datos = datos;

    public Task<Prestamo?> ObtenerPorIdAsync(int id) => Task.FromResult(datos.Prestamos.FirstOrDefault(p => p.Id == id));

    public Task<int> ContarActivosDeUsuarioAsync(int usuarioId) =>
        Task.FromResult(datos.Prestamos.Count(p => p.UsuarioId == usuarioId && p.EstaActivo));

    public Task<IReadOnlyList<Prestamo>> ListarActivosDeUsuarioAsync(int usuarioId) =>
        Task.FromResult<IReadOnlyList<Prestamo>>(datos.Prestamos.Where(p => p.UsuarioId == usuarioId && p.EstaActivo).ToList());

    public Task<IReadOnlyList<PrestamoDetalle>> ListarActivosDetalleAsync(string? texto, int? usuarioId)
    {
        var lista = datos.Prestamos
            .Where(p => p.EstaActivo && (usuarioId == null || p.UsuarioId == usuarioId))
            .Select(p => new PrestamoDetalle { Id = p.Id, UsuarioId = p.UsuarioId, FechaVencimiento = p.FechaVencimiento.ToDateTime(TimeOnly.MinValue) })
            .ToList();
        return Task.FromResult<IReadOnlyList<PrestamoDetalle>>(lista);
    }

    public Task<int> AgregarAsync(Prestamo prestamo) => throw new NotSupportedException();

    public Task ActualizarAsync(Prestamo prestamo) => throw new NotSupportedException();

    // Imitan a los stored procedures.
    public Task<int> RegistrarAsync(int usuarioId, int materialId, int registradoPorId)
    {
        var id = datos.Prestamos.Count + 1;
        var hoy = DateTime.Now;
        datos.Prestamos.Add(new Prestamo(id, usuarioId, materialId, hoy, DateOnly.FromDateTime(hoy).AddDays(7), null, false));
        datos.LlamadasProcedimientos.Add("prestar " + materialId);
        return Task.FromResult(id);
    }

    public Task<ResultadoDevolucion> DevolverAsync(int prestamoId, int registradoPorId)
    {
        datos.LlamadasProcedimientos.Add("devolver " + prestamoId);
        return Task.FromResult(new ResultadoDevolucion(3, 150m));
    }

    public Task RenovarAsync(int prestamoId, int registradoPorId)
    {
        datos.LlamadasProcedimientos.Add("renovar " + prestamoId);
        return Task.CompletedTask;
    }
}
