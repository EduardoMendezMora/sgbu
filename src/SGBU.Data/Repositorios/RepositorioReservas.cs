using System.Data;
using SGBU.Data.Infraestructura;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

internal sealed class RepositorioReservas : IRepositorioReservas
{
    private readonly ContextoSql contexto;

    public RepositorioReservas(ContextoSql contexto) => this.contexto = contexto;

    public async Task<bool> ExistePendienteAsync(int materialId, CancellationToken ct = default)
    {
        await using var comando = contexto.Comando("""
            SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Reservas WHERE MaterialId = @m AND Estado = N'Pendiente')
                        THEN 1 ELSE 0 END;
            """).Con("@m", SqlDbType.Int, materialId);
        return (int)(await comando.ExecuteScalarAsync(ct))! == 1;
    }

    public Task<int> AgregarAsync(Reserva reserva, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            INSERT dbo.Reservas (UsuarioId, MaterialId, FechaReserva, Estado)
            OUTPUT INSERTED.Id
            VALUES (@usuario, @material, @fecha, @estado);
            """)
            .Con("@usuario", SqlDbType.Int, reserva.UsuarioId)
            .Con("@material", SqlDbType.Int, reserva.MaterialId)
            .Con("@fecha", SqlDbType.DateTime2, reserva.FechaReserva)
            .Con("@estado", SqlDbType.NVarChar, reserva.Estado.ToString(), 12);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                return (int)(await comando.ExecuteScalarAsync(ct))!;
            }
        });
    }

    public Task ActualizarAsync(Reserva reserva, CancellationToken ct = default)
    {
        var comando = contexto.Comando("UPDATE dbo.Reservas SET Estado = @estado WHERE Id = @id;")
            .Con("@id", SqlDbType.Int, reserva.Id)
            .Con("@estado", SqlDbType.NVarChar, reserva.Estado.ToString(), 12);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                await comando.ExecuteNonQueryAsync(ct);
            }
        });
    }
}
