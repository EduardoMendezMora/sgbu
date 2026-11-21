using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

public class RepositorioReservas : IRepositorioReservas
{
    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;

    public RepositorioReservas(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;
    }

    public async Task<bool> ExistePendienteAsync(int materialId)
    {
        string sql = "SELECT COUNT(*) FROM Reservas WHERE MaterialId = @MaterialId AND Estado = 'Pendiente'";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@MaterialId", SqlDbType.Int).Value = materialId;
        int cantidad = (int)(await cmd.ExecuteScalarAsync())!;
        return cantidad > 0;
    }

    public async Task<int> AgregarAsync(Reserva reserva)
    {
        string sql = "INSERT INTO Reservas (UsuarioId, MaterialId, FechaReserva, Estado) OUTPUT INSERTED.Id " +
                     "VALUES (@UsuarioId, @MaterialId, @FechaReserva, @Estado)";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = reserva.UsuarioId;
        cmd.Parameters.Add("@MaterialId", SqlDbType.Int).Value = reserva.MaterialId;
        cmd.Parameters.Add("@FechaReserva", SqlDbType.DateTime2).Value = reserva.FechaReserva;
        cmd.Parameters.Add("@Estado", SqlDbType.NVarChar, 12).Value = reserva.Estado.ToString();
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task ActualizarAsync(Reserva reserva)
    {
        using SqlCommand cmd = new SqlCommand("UPDATE Reservas SET Estado = @Estado WHERE Id = @Id", conexion, transaccion);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = reserva.Id;
        cmd.Parameters.Add("@Estado", SqlDbType.NVarChar, 12).Value = reserva.Estado.ToString();
        await cmd.ExecuteNonQueryAsync();
    }
}
