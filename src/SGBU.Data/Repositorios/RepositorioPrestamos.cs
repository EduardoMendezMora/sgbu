using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

public class RepositorioPrestamos : IRepositorioPrestamos
{
    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;

    public RepositorioPrestamos(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;
    }

    public async Task<Prestamo?> ObtenerPorIdAsync(int id)
    {
        using SqlCommand cmd = new SqlCommand("SELECT * FROM Prestamos WHERE Id = @Id", conexion, transaccion);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return LeerPrestamo(reader);
        }

        return null;
    }

    // Un préstamo está activo mientras FechaDevolucion sea NULL.
    public async Task<int> ContarActivosDeUsuarioAsync(int usuarioId)
    {
        string sql = "SELECT COUNT(*) FROM Prestamos WHERE UsuarioId = @UsuarioId AND FechaDevolucion IS NULL";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<IReadOnlyList<Prestamo>> ListarActivosDeUsuarioAsync(int usuarioId)
    {
        string sql = "SELECT * FROM Prestamos WHERE UsuarioId = @UsuarioId AND FechaDevolucion IS NULL ORDER BY FechaVencimiento";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;

        List<Prestamo> lista = new List<Prestamo>();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(LeerPrestamo(reader));
        }

        return lista;
    }

    public async Task<int> AgregarAsync(Prestamo prestamo)
    {
        string sql = "INSERT INTO Prestamos (UsuarioId, MaterialId, FechaPrestamo, FechaVencimiento, FechaDevolucion, Renovado, RegistradoPorId) " +
                     "OUTPUT INSERTED.Id " +
                     "VALUES (@UsuarioId, @MaterialId, @FechaPrestamo, @FechaVencimiento, @FechaDevolucion, @Renovado, @RegistradoPorId)";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = prestamo.UsuarioId;
        cmd.Parameters.Add("@MaterialId", SqlDbType.Int).Value = prestamo.MaterialId;
        cmd.Parameters.Add("@FechaPrestamo", SqlDbType.DateTime2).Value = prestamo.FechaPrestamo;
        cmd.Parameters.Add("@FechaVencimiento", SqlDbType.Date).Value = prestamo.FechaVencimiento.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@FechaDevolucion", SqlDbType.DateTime2).Value = (object?)prestamo.FechaDevolucion ?? DBNull.Value;
        cmd.Parameters.Add("@Renovado", SqlDbType.Bit).Value = prestamo.Renovado;
        cmd.Parameters.Add("@RegistradoPorId", SqlDbType.Int).Value = (object?)prestamo.RegistradoPorId ?? DBNull.Value;
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    // Lo único que cambia de un préstamo es el vencimiento (al renovar) y la devolución.
    public async Task ActualizarAsync(Prestamo prestamo)
    {
        string sql = "UPDATE Prestamos SET FechaVencimiento = @FechaVencimiento, FechaDevolucion = @FechaDevolucion, " +
                     "Renovado = @Renovado WHERE Id = @Id";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = prestamo.Id;
        cmd.Parameters.Add("@FechaVencimiento", SqlDbType.Date).Value = prestamo.FechaVencimiento.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add("@FechaDevolucion", SqlDbType.DateTime2).Value = (object?)prestamo.FechaDevolucion ?? DBNull.Value;
        cmd.Parameters.Add("@Renovado", SqlDbType.Bit).Value = prestamo.Renovado;
        await cmd.ExecuteNonQueryAsync();
    }

    private static Prestamo LeerPrestamo(SqlDataReader reader)
    {
        Prestamo prestamo = new Prestamo(
            (int)reader["Id"],
            (int)reader["UsuarioId"],
            (int)reader["MaterialId"],
            (DateTime)reader["FechaPrestamo"],
            DateOnly.FromDateTime((DateTime)reader["FechaVencimiento"]),
            reader["FechaDevolucion"] as DateTime?,
            (bool)reader["Renovado"])
        {
            RegistradoPorId = reader["RegistradoPorId"] as int?,
        };

        return prestamo;
    }
}
