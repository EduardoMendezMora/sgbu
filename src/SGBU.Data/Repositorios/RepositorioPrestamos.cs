using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
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

    public async Task<IReadOnlyList<PrestamoDetalle>> ListarActivosDetalleAsync(string? texto, int? usuarioId)
    {
        string sql = "SELECT p.Id, p.UsuarioId, u.Carne, u.Nombre + ' ' + u.Apellidos AS Usuario, m.Isbn, m.Titulo, " +
                     "p.FechaPrestamo, p.FechaVencimiento, p.Renovado " +
                     "FROM Prestamos p " +
                     "INNER JOIN Usuarios u ON u.Id = p.UsuarioId " +
                     "INNER JOIN Materiales m ON m.Id = p.MaterialId " +
                     "WHERE p.FechaDevolucion IS NULL " +
                     "AND (@UsuarioId IS NULL OR p.UsuarioId = @UsuarioId) " +
                     "AND (@Texto IS NULL OR u.Carne LIKE @Texto OR u.Nombre LIKE @Texto OR u.Apellidos LIKE @Texto " +
                     "OR m.Titulo LIKE @Texto OR m.Isbn LIKE @Texto) " +
                     "ORDER BY p.FechaVencimiento";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = (object?)usuarioId ?? DBNull.Value;
        cmd.Parameters.Add("@Texto", SqlDbType.NVarChar, 210).Value =
            string.IsNullOrWhiteSpace(texto) ? DBNull.Value : "%" + texto.Trim() + "%";

        List<PrestamoDetalle> lista = new List<PrestamoDetalle>();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            PrestamoDetalle detalle = new PrestamoDetalle();
            detalle.Id = (int)reader["Id"];
            detalle.UsuarioId = (int)reader["UsuarioId"];
            detalle.Carne = (string)reader["Carne"];
            detalle.Usuario = (string)reader["Usuario"];
            detalle.Isbn = (string)reader["Isbn"];
            detalle.Titulo = (string)reader["Titulo"];
            detalle.FechaPrestamo = (DateTime)reader["FechaPrestamo"];
            detalle.FechaVencimiento = (DateTime)reader["FechaVencimiento"];
            detalle.Renovado = (bool)reader["Renovado"];
            lista.Add(detalle);
        }

        return lista;
    }

    public async Task<int> RegistrarAsync(int usuarioId, int materialId, int registradoPorId)
    {
        using SqlCommand cmd = new SqlCommand("sp_RegistrarPrestamo", conexion, transaccion);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
        cmd.Parameters.Add("@MaterialId", SqlDbType.Int).Value = materialId;
        cmd.Parameters.Add("@RegistradoPorId", SqlDbType.Int).Value = registradoPorId;
        SqlParameter salida = cmd.Parameters.Add("@PrestamoId", SqlDbType.Int);
        salida.Direction = ParameterDirection.Output;

        await EjecutarProcedimiento(cmd);
        return (int)salida.Value;
    }

    public async Task<ResultadoDevolucion> DevolverAsync(int prestamoId, int registradoPorId)
    {
        using SqlCommand cmd = new SqlCommand("sp_RegistrarDevolucion", conexion, transaccion);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
        cmd.Parameters.Add("@RegistradoPorId", SqlDbType.Int).Value = registradoPorId;
        SqlParameter dias = cmd.Parameters.Add("@DiasAtraso", SqlDbType.Int);
        dias.Direction = ParameterDirection.Output;
        SqlParameter monto = cmd.Parameters.Add("@Monto", SqlDbType.Decimal);
        monto.Precision = 10;
        monto.Scale = 2;
        monto.Direction = ParameterDirection.Output;

        await EjecutarProcedimiento(cmd);
        return new ResultadoDevolucion((int)dias.Value, (decimal)monto.Value);
    }

    public async Task RenovarAsync(int prestamoId, int registradoPorId)
    {
        using SqlCommand cmd = new SqlCommand("sp_RenovarPrestamo", conexion, transaccion);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
        cmd.Parameters.Add("@RegistradoPorId", SqlDbType.Int).Value = registradoPorId;

        await EjecutarProcedimiento(cmd);
    }

    // Los procedimientos avisan los errores con THROW 50001, 50002... y un mensaje para el usuario.
    private static async Task EjecutarProcedimiento(SqlCommand cmd)
    {
        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number > 50000)
        {
            throw new OperacionNoPermitidaException(ex.Message);
        }
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
