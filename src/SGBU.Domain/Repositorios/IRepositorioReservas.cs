using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

public interface IRepositorioReservas
{
    /// <summary>Para la regla de renovación: ¿alguien más tiene reservado este material?</summary>
    Task<bool> ExistePendienteAsync(int materialId);

    Task<int> AgregarAsync(Reserva reserva);

    Task ActualizarAsync(Reserva reserva);
}
