using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioReservas
{
    /// <summary>Para la regla de renovación: ¿alguien más tiene reservado este material?</summary>
    Task<bool> ExistePendienteAsync(int materialId, CancellationToken ct = default);

    Task<int> AgregarAsync(Reserva reserva, CancellationToken ct = default);

    Task ActualizarAsync(Reserva reserva, CancellationToken ct = default);
}
