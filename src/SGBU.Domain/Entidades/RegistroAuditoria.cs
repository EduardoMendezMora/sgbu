using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

/// <summary>Una fila de la bitácora: quién, qué acción y cuándo. No cambia después de creada.</summary>
public sealed class RegistroAuditoria
{
    public RegistroAuditoria(int? usuarioId, string accion, string? entidad, int? entidadId, string? detalle, DateTime fecha)
    {
        UsuarioId = usuarioId;
        Accion = Requerir.Texto(accion, "La acción").ToUpperInvariant();
        Entidad = Requerir.TextoOpcional(entidad);
        EntidadId = entidadId;
        Detalle = Requerir.TextoOpcional(detalle);
        Fecha = fecha;
    }

    public long Id { get; init; }

    public int? UsuarioId { get; }

    public string Accion { get; }

    public string? Entidad { get; }

    public int? EntidadId { get; }

    public string? Detalle { get; }

    public DateTime Fecha { get; }
}
