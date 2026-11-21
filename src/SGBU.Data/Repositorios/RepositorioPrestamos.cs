using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Data.Infraestructura;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

/// <summary>
/// Préstamos. "Activo" significa FechaDevolucion IS NULL; esas consultas usan el índice
/// filtrado IX_Prestamos_Usuario_Activos de 004_indices.sql.
/// </summary>
internal sealed class RepositorioPrestamos : IRepositorioPrestamos
{
    private const string Consulta = """
        SELECT Id, UsuarioId, MaterialId, FechaPrestamo, FechaVencimiento, FechaDevolucion, Renovado, RegistradoPorId
        FROM dbo.Prestamos
        """;

    private readonly ContextoSql contexto;

    public RepositorioPrestamos(ContextoSql contexto) => this.contexto = contexto;

    public async Task<Prestamo?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var lista = await LeerAsync(contexto.Comando($"{Consulta} WHERE Id = @id;").Con("@id", SqlDbType.Int, id), ct);
        return lista.Count == 0 ? null : lista[0];
    }

    public async Task<int> ContarActivosDeUsuarioAsync(int usuarioId, CancellationToken ct = default)
    {
        await using var comando = contexto.Comando(
            "SELECT COUNT(*) FROM dbo.Prestamos WHERE UsuarioId = @u AND FechaDevolucion IS NULL;")
            .Con("@u", SqlDbType.Int, usuarioId);
        return (int)(await comando.ExecuteScalarAsync(ct))!;
    }

    public Task<IReadOnlyList<Prestamo>> ListarActivosDeUsuarioAsync(int usuarioId, CancellationToken ct = default) =>
        LeerAsync(
            contexto.Comando($"{Consulta} WHERE UsuarioId = @u AND FechaDevolucion IS NULL ORDER BY FechaVencimiento;")
                .Con("@u", SqlDbType.Int, usuarioId),
            ct);

    public Task<int> AgregarAsync(Prestamo prestamo, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            INSERT dbo.Prestamos (UsuarioId, MaterialId, FechaPrestamo, FechaVencimiento, FechaDevolucion, Renovado, RegistradoPorId)
            OUTPUT INSERTED.Id
            VALUES (@usuario, @material, @fecha, @vence, @devuelto, @renovado, @registra);
            """)
            .Con("@usuario", SqlDbType.Int, prestamo.UsuarioId)
            .Con("@material", SqlDbType.Int, prestamo.MaterialId)
            .Con("@fecha", SqlDbType.DateTime2, prestamo.FechaPrestamo)
            .ConFecha("@vence", prestamo.FechaVencimiento)
            .Con("@devuelto", SqlDbType.DateTime2, prestamo.FechaDevolucion)
            .Con("@renovado", SqlDbType.Bit, prestamo.Renovado)
            .Con("@registra", SqlDbType.Int, prestamo.RegistradoPorId);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                return (int)(await comando.ExecuteScalarAsync(ct))!;
            }
        });
    }

    /// <summary>Lo único que cambia de un préstamo: el vencimiento (renovar) y la devolución.</summary>
    public Task ActualizarAsync(Prestamo prestamo, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            UPDATE dbo.Prestamos SET FechaVencimiento = @vence, FechaDevolucion = @devuelto, Renovado = @renovado
            WHERE Id = @id;
            """)
            .Con("@id", SqlDbType.Int, prestamo.Id)
            .ConFecha("@vence", prestamo.FechaVencimiento)
            .Con("@devuelto", SqlDbType.DateTime2, prestamo.FechaDevolucion)
            .Con("@renovado", SqlDbType.Bit, prestamo.Renovado);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                await comando.ExecuteNonQueryAsync(ct);
            }
        });
    }

    private static async Task<IReadOnlyList<Prestamo>> LeerAsync(SqlCommand comando, CancellationToken ct)
    {
        await using (comando)
        {
            await using var lector = await comando.ExecuteReaderAsync(ct);
            var lista = new List<Prestamo>();
            while (await lector.ReadAsync(ct))
            {
                lista.Add(new Prestamo(
                    lector.Entero("Id"), lector.Entero("UsuarioId"), lector.Entero("MaterialId"),
                    lector.Fecha("FechaPrestamo"), lector.SoloFecha("FechaVencimiento"),
                    lector.FechaONula("FechaDevolucion"), lector.Booleano("Renovado"))
                {
                    RegistradoPorId = lector.EnteroONulo("RegistradoPorId"),
                });
            }

            return lista;
        }
    }
}
