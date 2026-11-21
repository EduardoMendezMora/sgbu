using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class MultasFalsas : IRepositorioMultas
{
    private readonly FabricaFalsa datos;

    public MultasFalsas(FabricaFalsa datos) => this.datos = datos;

    public Task<bool> TieneMultasPendientesAsync(int usuarioId) =>
        Task.FromResult(datos.UsuariosConMulta.Contains(usuarioId));

    public Task<IReadOnlyList<Multa>> ListarPendientesDeUsuarioAsync(int usuarioId) => throw new NotSupportedException();

    public Task<int> AgregarAsync(Multa multa) => throw new NotSupportedException();

    public Task ActualizarAsync(Multa multa) => throw new NotSupportedException();
}
