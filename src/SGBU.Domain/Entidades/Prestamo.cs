using SGBU.Domain.Excepciones;

namespace SGBU.Domain.Entidades;

/// <summary>
/// Un ejemplar prestado a un usuario.
/// Ningún método usa DateTime.Now por dentro: la fecha "de hoy" se recibe como parámetro.
/// Así las pruebas pueden fijar la fecha y dar siempre el mismo resultado.
/// </summary>
public sealed class Prestamo
{
    public Prestamo(int id, int usuarioId, int materialId, DateTime fechaPrestamo, DateOnly fechaVencimiento, DateTime? fechaDevolucion, bool renovado)
    {
        if (fechaVencimiento < DateOnly.FromDateTime(fechaPrestamo))
        {
            throw new DatoInvalidoException("El vencimiento no puede ser antes de la fecha del préstamo.");
        }

        Id = id;
        UsuarioId = usuarioId;
        MaterialId = materialId;
        FechaPrestamo = fechaPrestamo;
        FechaVencimiento = fechaVencimiento;
        FechaDevolucion = fechaDevolucion;
        Renovado = renovado;
    }

    public int Id { get; }

    public int UsuarioId { get; }

    public int MaterialId { get; }

    public int? RegistradoPorId { get; init; }

    public DateTime FechaPrestamo { get; }

    public DateOnly FechaVencimiento { get; private set; }

    public DateTime? FechaDevolucion { get; private set; }

    public bool Renovado { get; private set; }

    public bool EstaActivo => FechaDevolucion is null;

    /// <summary>Préstamo nuevo: vence según los días que da el rol del usuario.</summary>
    public static Prestamo Crear(int usuarioId, int materialId, Rol rol, DateTime ahora)
    {
        var vencimiento = DateOnly.FromDateTime(ahora).AddDays(rol.DiasPrestamo);
        return new Prestamo(0, usuarioId, materialId, ahora, vencimiento, null, false);
    }

    public int DiasAtraso(DateOnly fecha) => Math.Max(0, fecha.DayNumber - FechaVencimiento.DayNumber);

    /// <summary>Marca la devolución y devuelve los días de atraso (0 si llegó a tiempo).</summary>
    public int Devolver(DateTime ahora)
    {
        if (!EstaActivo)
        {
            throw new OperacionNoPermitidaException("Este préstamo ya fue devuelto.");
        }

        FechaDevolucion = ahora;
        return DiasAtraso(DateOnly.FromDateTime(ahora));
    }

    /// <summary>
    /// Regla: una sola renovación y solo si nadie reservó el material.
    /// Además no se renueva un préstamo vencido: primero se devuelve y se cobra la multa.
    /// </summary>
    public void Renovar(Rol rol, bool materialReservado, DateTime ahora)
    {
        if (!EstaActivo)
        {
            throw new RenovacionNoPermitidaException("el préstamo ya fue devuelto.");
        }

        if (Renovado)
        {
            throw new RenovacionNoPermitidaException("el préstamo ya se renovó una vez.");
        }

        if (materialReservado)
        {
            throw new RenovacionNoPermitidaException("otro usuario reservó este material.");
        }

        if (DiasAtraso(DateOnly.FromDateTime(ahora)) > 0)
        {
            throw new RenovacionNoPermitidaException("el préstamo está vencido.");
        }

        FechaVencimiento = FechaVencimiento.AddDays(rol.DiasPrestamo);
        Renovado = true;
    }
}
