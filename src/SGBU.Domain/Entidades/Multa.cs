using SGBU.Domain.Excepciones;
using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

/// <summary>
/// Multa por devolver tarde: ₡50 por día, con tope de ₡2000.
/// El monto es decimal (nunca double/float): el dinero necesita exactitud, y double guarda
/// 0.1 como 0.1000000000000000055...
/// </summary>
public sealed class Multa
{
    public Multa(int id, int prestamoId, int usuarioId, int diasAtraso, decimal monto, EstadoMulta estado, DateTime fechaGeneracion, DateTime? fechaPago)
    {
        if (monto < 0 || monto > ReglasBiblioteca.TopeMulta)
        {
            throw new DatoInvalidoException($"El monto de la multa debe estar entre ₡0 y ₡{ReglasBiblioteca.TopeMulta:N0}.");
        }

        Id = id;
        PrestamoId = prestamoId;
        UsuarioId = usuarioId;
        DiasAtraso = diasAtraso;
        Monto = monto;
        Estado = estado;
        FechaGeneracion = fechaGeneracion;
        FechaPago = fechaPago;
    }

    public int Id { get; }

    public int PrestamoId { get; }

    public int UsuarioId { get; }

    public int DiasAtraso { get; }

    public decimal Monto { get; }

    public EstadoMulta Estado { get; private set; }

    public DateTime FechaGeneracion { get; }

    public DateTime? FechaPago { get; private set; }

    public static decimal CalcularMonto(int diasAtraso)
    {
        if (diasAtraso < 0)
        {
            throw new DatoInvalidoException("Los días de atraso no pueden ser negativos.");
        }

        return Math.Min(diasAtraso * ReglasBiblioteca.MultaPorDia, ReglasBiblioteca.TopeMulta);
    }

    /// <summary>Crea la multa al devolver un préstamo; si no hubo atraso devuelve null.</summary>
    public static Multa? Generar(Prestamo prestamo, DateTime fechaDevolucion)
    {
        var dias = prestamo.DiasAtraso(DateOnly.FromDateTime(fechaDevolucion));
        if (dias == 0)
        {
            return null;
        }

        return new Multa(0, prestamo.Id, prestamo.UsuarioId, dias, CalcularMonto(dias), EstadoMulta.Pendiente, fechaDevolucion, null);
    }

    public void Pagar(DateTime ahora)
    {
        if (Estado == EstadoMulta.Pagada)
        {
            throw new OperacionNoPermitidaException("La multa ya está pagada.");
        }

        Estado = EstadoMulta.Pagada;
        FechaPago = ahora;
    }
}
