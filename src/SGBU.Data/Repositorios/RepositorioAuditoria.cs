using System.Data;
using SGBU.Data.Infraestructura;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

/// <summary>
/// Bitácora. Va dentro de la misma transacción que la operación: si el préstamo se revierte,
/// su registro de auditoría también, y nunca queda un "PRESTAMO" que no ocurrió.
/// </summary>
internal sealed class RepositorioAuditoria : IRepositorioAuditoria
{
    private readonly ContextoSql contexto;

    public RepositorioAuditoria(ContextoSql contexto) => this.contexto = contexto;

    public async Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct = default)
    {
        await using var comando = contexto.Comando("""
            INSERT dbo.Auditoria (UsuarioId, Accion, Entidad, EntidadId, Detalle, Fecha)
            VALUES (@usuario, @accion, @entidad, @entidadId, @detalle, @fecha);
            """)
            .Con("@usuario", SqlDbType.Int, registro.UsuarioId)
            .Con("@accion", SqlDbType.NVarChar, registro.Accion, 50)
            .Con("@entidad", SqlDbType.NVarChar, registro.Entidad, 50)
            .Con("@entidadId", SqlDbType.Int, registro.EntidadId)
            .Con("@detalle", SqlDbType.NVarChar, registro.Detalle, 500)
            .Con("@fecha", SqlDbType.DateTime2, registro.Fecha);
        await comando.ExecuteNonQueryAsync(ct);
    }
}
