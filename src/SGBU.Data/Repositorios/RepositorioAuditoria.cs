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

    public async Task<Pagina<AuditoriaDetalle>> BuscarAsync(string? texto, DateTime desde, DateTime hasta, int numeroPagina, int tamanoPagina)
    {
        // "hasta" incluye todo ese día: se busca menor que el día siguiente.
        string filtro = " WHERE a.Fecha >= @Desde AND a.Fecha < @Hasta " +
                        "AND (@Texto IS NULL OR a.Accion LIKE @Texto OR u.Nombre LIKE @Texto OR u.Apellidos LIKE @Texto OR u.Carne LIKE @Texto)";
        object valorTexto = string.IsNullOrWhiteSpace(texto) ? DBNull.Value : "%" + texto.Trim() + "%";

        using SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Auditoria a LEFT JOIN Usuarios u ON u.Id = a.UsuarioId" + filtro, conexion, transaccion);
        AgregarFiltros(cmdTotal, valorTexto, desde, hasta);
        int total = (int)(await cmdTotal.ExecuteScalarAsync())!;

        // LEFT JOIN porque hay registros sin usuario (por ejemplo, un login con un correo que no existe).
        string sql = "SELECT a.Fecha, ISNULL(u.Carne + ' - ' + u.Nombre + ' ' + u.Apellidos, '(sistema)') AS Usuario, " +
                     "a.Accion, ISNULL(a.Entidad, '') AS Entidad, ISNULL(a.Detalle, '') AS Detalle " +
                     "FROM Auditoria a LEFT JOIN Usuarios u ON u.Id = a.UsuarioId" + filtro +
                     " ORDER BY a.Fecha DESC, a.Id DESC OFFSET @Saltar ROWS FETCH NEXT @Tamano ROWS ONLY";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        AgregarFiltros(cmd, valorTexto, desde, hasta);
        cmd.Parameters.Add("@Saltar", SqlDbType.Int).Value = (numeroPagina - 1) * tamanoPagina;
        cmd.Parameters.Add("@Tamano", SqlDbType.Int).Value = tamanoPagina;

        List<AuditoriaDetalle> lista = new List<AuditoriaDetalle>();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            AuditoriaDetalle detalle = new AuditoriaDetalle();
            detalle.Fecha = (DateTime)reader["Fecha"];
            detalle.Usuario = (string)reader["Usuario"];
            detalle.Accion = (string)reader["Accion"];
            detalle.Entidad = (string)reader["Entidad"];
            detalle.Detalle = (string)reader["Detalle"];
            lista.Add(detalle);
        }

        return new Pagina<AuditoriaDetalle>(lista, numeroPagina, tamanoPagina, total);
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

    private static void AgregarFiltros(SqlCommand cmd, object valorTexto, DateTime desde, DateTime hasta)
    {
        cmd.Parameters.Add("@Texto", SqlDbType.NVarChar, 130).Value = valorTexto;
        cmd.Parameters.Add("@Desde", SqlDbType.DateTime2).Value = desde.Date;
        cmd.Parameters.Add("@Hasta", SqlDbType.DateTime2).Value = hasta.Date.AddDays(1);
    }
}
