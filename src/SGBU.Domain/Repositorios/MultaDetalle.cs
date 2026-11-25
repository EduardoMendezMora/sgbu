namespace SGBU.Domain.Repositorios;

// Una multa pendiente con los datos del usuario y del material, para mostrarla en una tabla.
public class MultaDetalle
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Carne { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public int DiasAtraso { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaGeneracion { get; set; }
}
