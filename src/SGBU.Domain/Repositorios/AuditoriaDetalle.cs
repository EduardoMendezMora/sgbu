namespace SGBU.Domain.Repositorios;

// Un registro de la bitácora con el nombre del usuario, para la pantalla de auditoría.
public class AuditoriaDetalle
{
    public DateTime Fecha { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string Accion { get; set; } = string.Empty;

    public string Entidad { get; set; } = string.Empty;

    public string Detalle { get; set; } = string.Empty;
}
