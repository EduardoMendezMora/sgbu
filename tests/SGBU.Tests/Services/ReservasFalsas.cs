using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class ReservasFalsas : IRepositorioReservas
{
    private readonly FabricaFalsa datos;

    public ReservasFalsas(FabricaFalsa datos) => this.datos = datos;

    public Task<bool> ExistePendienteAsync(int materialId) => Task.FromResult(datos.MaterialesReservados.Contains(materialId));

    public Task<int> AgregarAsync(Reserva reserva) => throw new NotSupportedException();

    public Task ActualizarAsync(Reserva reserva) => throw new NotSupportedException();
}
