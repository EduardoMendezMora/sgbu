namespace SGBU.Domain.Repositorios;

// Un préstamo con los datos del usuario y del material, para mostrarlo en una tabla.
public class PrestamoDetalle
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Carne { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;

    public string Isbn { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public DateTime FechaPrestamo { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public bool Renovado { get; set; }

    // Días que lleva vencido (0 si todavía está a tiempo).
    public int DiasAtraso
    {
        get
        {
            int dias = (DateTime.Today - FechaVencimiento.Date).Days;
            return dias > 0 ? dias : 0;
        }
    }
}
