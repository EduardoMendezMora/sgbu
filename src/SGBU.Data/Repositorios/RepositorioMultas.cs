using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

public class RepositorioMultas : IRepositorioMultas
{
    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;

    public RepositorioMultas(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;
    }

    public async Task<bool> TieneMultasPendientesAsync(int usuarioId)
    {
        string sql = "SELECT COUNT(*) FROM Multas WHERE UsuarioId = @UsuarioId AND Estado = 'Pendiente'";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
        int cantidad = (int)(await cmd.ExecuteScalarAsync())!;
        return cantidad > 0;
    }

    public async Task<IReadOnlyList<Multa>> ListarPendientesDeUsuarioAsync(int usuarioId)
    {
        string sql = "SELECT * FROM Multas WHERE UsuarioId = @UsuarioId AND Estado = 'Pendiente' ORDER BY FechaGeneracion";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;

        List<Multa> lista = new List<Multa>();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            EstadoMulta estado = (string)reader["Estado"] == "Pagada" ? EstadoMulta.Pagada : EstadoMulta.Pendiente;
            lista.Add(new Multa(
                (int)reader["Id"],
                (int)reader["PrestamoId"],
                (int)reader["UsuarioId"],
                (int)reader["DiasAtraso"],
                (decimal)reader["Monto"],
                estado,
                (DateTime)reader["FechaGeneracion"],
                reader["FechaPago"] as DateTime?));
        }

        return lista;
    }

    public async Task<int> AgregarAsync(Multa multa)
    {
        string sql = "INSERT INTO Multas (PrestamoId, UsuarioId, DiasAtraso, Monto, Estado, FechaGeneracion, FechaPago) " +
                     "OUTPUT INSERTED.Id " +
                     "VALUES (@PrestamoId, @UsuarioId, @DiasAtraso, @Monto, @Estado, @FechaGeneracion, @FechaPago)";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = multa.PrestamoId;
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = multa.UsuarioId;
        cmd.Parameters.Add("@DiasAtraso", SqlDbType.Int).Value = multa.DiasAtraso;
        cmd.Parameters.Add("@Monto", SqlDbType.Decimal).Value = multa.Monto;
        cmd.Parameters.Add("@Estado", SqlDbType.NVarChar, 10).Value = multa.Estado.ToString();
        cmd.Parameters.Add("@FechaGeneracion", SqlDbType.DateTime2).Value = multa.FechaGeneracion;
        cmd.Parameters.Add("@FechaPago", SqlDbType.DateTime2).Value = (object?)multa.FechaPago ?? DBNull.Value;

        try
        {
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new DatoInvalidoException("Ese préstamo ya tiene una multa.");
        }
    }

    // Al pagar solo cambian el estado y la fecha de pago.
    public async Task ActualizarAsync(Multa multa)
    {
        string sql = "UPDATE Multas SET Estado = @Estado, FechaPago = @FechaPago WHERE Id = @Id";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = multa.Id;
        cmd.Parameters.Add("@Estado", SqlDbType.NVarChar, 10).Value = multa.Estado.ToString();
        cmd.Parameters.Add("@FechaPago", SqlDbType.DateTime2).Value = (object?)multa.FechaPago ?? DBNull.Value;
        await cmd.ExecuteNonQueryAsync();
    }
}
