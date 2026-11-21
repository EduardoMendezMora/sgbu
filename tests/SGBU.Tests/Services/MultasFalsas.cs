using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class MultasFalsas : IRepositorioMultas
{
    private readonly FabricaFalsa datos;

    public MultasFalsas(FabricaFalsa datos) => this.datos = datos;

    public Task<Multa?> ObtenerPorIdAsync(int id) => Task.FromResult(datos.Multas.FirstOrDefault(m => m.Id == id));

    public Task<bool> TieneMultasPendientesAsync(int usuarioId) =>
        Task.FromResult(datos.UsuariosConMulta.Contains(usuarioId)
            || datos.Multas.Any(m => m.UsuarioId == usuarioId && m.Estado == EstadoMulta.Pendiente));

    public Task<IReadOnlyList<MultaDetalle>> ListarPendientesDetalleAsync(string? texto, int? usuarioId)
    {
        var lista = datos.Multas
            .Where(m => m.Estado == EstadoMulta.Pendiente && (usuarioId == null || m.UsuarioId == usuarioId))
            .Select(m => new MultaDetalle { Id = m.Id, UsuarioId = m.UsuarioId, Monto = m.Monto, DiasAtraso = m.DiasAtraso })
            .ToList();
        return Task.FromResult<IReadOnlyList<MultaDetalle>>(lista);
    }

    // Imita a sp_PagarMulta.
    public Task PagarAsync(int multaId, int registradoPorId)
    {
        datos.Multas.Single(m => m.Id == multaId).Pagar(DateTime.Now);
        datos.LlamadasProcedimientos.Add("pagar " + multaId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Multa>> ListarPendientesDeUsuarioAsync(int usuarioId) => throw new NotSupportedException();

    public Task<int> AgregarAsync(Multa multa) => throw new NotSupportedException();

    public Task ActualizarAsync(Multa multa) => throw new NotSupportedException();
}
