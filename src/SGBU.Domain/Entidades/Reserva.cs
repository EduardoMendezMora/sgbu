using SGBU.Domain.Excepciones;

namespace SGBU.Domain.Entidades;

/// <summary>Un usuario aparta un material que hoy no está disponible.</summary>
public sealed class Reserva
{
    public Reserva(int id, int usuarioId, int materialId, DateTime fechaReserva, EstadoReserva estado)
    {
        Id = id;
        UsuarioId = usuarioId;
        MaterialId = materialId;
        FechaReserva = fechaReserva;
        Estado = estado;
    }

    public int Id { get; }

    public int UsuarioId { get; }

    public int MaterialId { get; }

    public DateTime FechaReserva { get; }

    public EstadoReserva Estado { get; private set; }

    public static Reserva Crear(int usuarioId, int materialId, DateTime ahora) =>
        new(0, usuarioId, materialId, ahora, EstadoReserva.Pendiente);

    public void Atender() => CambiarDesdePendiente(EstadoReserva.Atendida);

    public void Cancelar() => CambiarDesdePendiente(EstadoReserva.Cancelada);

    private void CambiarDesdePendiente(EstadoReserva nuevo)
    {
        if (Estado != EstadoReserva.Pendiente)
        {
            throw new OperacionNoPermitidaException($"La reserva ya está {Estado.ToString().ToLowerInvariant()}.");
        }

        Estado = nuevo;
    }
}
