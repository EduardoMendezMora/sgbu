using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Data.Infraestructura;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

/// <summary>
/// Multas. TieneMultasPendientesAsync se llama en cada préstamo; usa EXISTS (se detiene en
/// la primera fila) y el índice filtrado IX_Multas_Usuario_Pendientes.
/// </summary>
internal sealed class RepositorioMultas : IRepositorioMultas
{
    private readonly ContextoSql contexto;

    public RepositorioMultas(ContextoSql contexto) => this.contexto = contexto;

    public async Task<bool> TieneMultasPendientesAsync(int usuarioId, CancellationToken ct = default)
    {
        await using var comando = contexto.Comando("""
            SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Multas WHERE UsuarioId = @u AND Estado = N'Pendiente')
                        THEN 1 ELSE 0 END;
            """).Con("@u", SqlDbType.Int, usuarioId);
        return (int)(await comando.ExecuteScalarAsync(ct))! == 1;
    }

    public async Task<IReadOnlyList<Multa>> ListarPendientesDeUsuarioAsync(int usuarioId, CancellationToken ct = default)
    {
        await using var comando = contexto.Comando("""
            SELECT Id, PrestamoId, UsuarioId, DiasAtraso, Monto, Estado, FechaGeneracion, FechaPago
            FROM dbo.Multas WHERE UsuarioId = @u AND Estado = N'Pendiente' ORDER BY FechaGeneracion;
            """).Con("@u", SqlDbType.Int, usuarioId);

        await using var lector = await comando.ExecuteReaderAsync(ct);
        var lista = new List<Multa>();
        while (await lector.ReadAsync(ct))
        {
            lista.Add(new Multa(
                lector.Entero("Id"), lector.Entero("PrestamoId"), lector.Entero("UsuarioId"), lector.Entero("DiasAtraso"),
                lector.Decimal("Monto"), Enum.Parse<EstadoMulta>(lector.Texto("Estado")),
                lector.Fecha("FechaGeneracion"), lector.FechaONula("FechaPago")));
        }

        return lista;
    }

    public Task<int> AgregarAsync(Multa multa, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            INSERT dbo.Multas (PrestamoId, UsuarioId, DiasAtraso, Monto, Estado, FechaGeneracion, FechaPago)
            OUTPUT INSERTED.Id
            VALUES (@prestamo, @usuario, @dias, @monto, @estado, @generada, @pago);
            """)
            .Con("@prestamo", SqlDbType.Int, multa.PrestamoId)
            .Con("@usuario", SqlDbType.Int, multa.UsuarioId)
            .Con("@dias", SqlDbType.Int, multa.DiasAtraso)
            .Con("@monto", SqlDbType.Decimal, multa.Monto)
            .Con("@estado", SqlDbType.NVarChar, multa.Estado.ToString(), 10)
            .Con("@generada", SqlDbType.DateTime2, multa.FechaGeneracion)
            .Con("@pago", SqlDbType.DateTime2, multa.FechaPago);
        comando.Parameters["@monto"].Precision = 10;
        comando.Parameters["@monto"].Scale = 2;

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                return (int)(await comando.ExecuteScalarAsync(ct))!;
            }
        });
    }

    /// <summary>Lo que cambia de una multa es su pago.</summary>
    public Task ActualizarAsync(Multa multa, CancellationToken ct = default)
    {
        var comando = contexto.Comando("UPDATE dbo.Multas SET Estado = @estado, FechaPago = @pago WHERE Id = @id;")
            .Con("@id", SqlDbType.Int, multa.Id)
            .Con("@estado", SqlDbType.NVarChar, multa.Estado.ToString(), 10)
            .Con("@pago", SqlDbType.DateTime2, multa.FechaPago);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                await comando.ExecuteNonQueryAsync(ct);
            }
        });
    }
}
