using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

public class RepositorioAuditoria : IRepositorioAuditoria
{
    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;

    public RepositorioAuditoria(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;
    }

    public async Task RegistrarAsync(RegistroAuditoria registro)
    {
        string sql = "INSERT INTO Auditoria (UsuarioId, Accion, Entidad, EntidadId, Detalle, Fecha) " +
                     "VALUES (@UsuarioId, @Accion, @Entidad, @EntidadId, @Detalle, @Fecha)";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = (object?)registro.UsuarioId ?? DBNull.Value;
        cmd.Parameters.Add("@Accion", SqlDbType.NVarChar, 50).Value = registro.Accion;
        cmd.Parameters.Add("@Entidad", SqlDbType.NVarChar, 50).Value = (object?)registro.Entidad ?? DBNull.Value;
        cmd.Parameters.Add("@EntidadId", SqlDbType.Int).Value = (object?)registro.EntidadId ?? DBNull.Value;
        cmd.Parameters.Add("@Detalle", SqlDbType.NVarChar, 500).Value = (object?)registro.Detalle ?? DBNull.Value;
        cmd.Parameters.Add("@Fecha", SqlDbType.DateTime2).Value = registro.Fecha;
        await cmd.ExecuteNonQueryAsync();
    }
}
