using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

// Cada reporte es una consulta. DataTable.Load llena la tabla con el resultado.
// Los nombres de columna (con [ ]) son los que se ven en la pantalla, el PDF y el Excel.
public class RepositorioReportes : IRepositorioReportes
{
    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;

    public RepositorioReportes(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;
    }

    public async Task<DataTable> TopMasPrestadosAsync()
    {
        string sql = "SELECT TOP 10 m.Titulo AS [Título], m.Autor, m.Tipo, COUNT(p.Id) AS [Veces prestado] " +
                     "FROM Prestamos p INNER JOIN Materiales m ON m.Id = p.MaterialId " +
                     "GROUP BY m.Titulo, m.Autor, m.Tipo " +
                     "ORDER BY COUNT(p.Id) DESC, m.Titulo";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        return await LlenarTabla(cmd);
    }

    public async Task<DataTable> UsuariosConMultasAsync()
    {
        string sql = "SELECT u.Carne AS [Carné], u.Nombre + ' ' + u.Apellidos AS Usuario, u.Email, " +
                     "COUNT(mu.Id) AS [Multas], SUM(mu.Monto) AS [Total pendiente] " +
                     "FROM Multas mu INNER JOIN Usuarios u ON u.Id = mu.UsuarioId " +
                     "WHERE mu.Estado = 'Pendiente' " +
                     "GROUP BY u.Carne, u.Nombre, u.Apellidos, u.Email " +
                     "ORDER BY SUM(mu.Monto) DESC";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        return await LlenarTabla(cmd);
    }

    public async Task<DataTable> StockCriticoAsync(int limite)
    {
        string sql = "SELECT Isbn AS ISBN, Titulo AS [Título], Tipo, StockDisponible AS Disponibles, StockTotal AS Total " +
                     "FROM Materiales WHERE Activo = 1 AND StockDisponible <= @Limite " +
                     "ORDER BY StockDisponible, Titulo";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@Limite", SqlDbType.Int).Value = limite;
        return await LlenarTabla(cmd);
    }

    public async Task<DataTable> VencidosHoyAsync()
    {
        string sql = "SELECT u.Carne AS [Carné], u.Nombre + ' ' + u.Apellidos AS Usuario, u.Telefono AS [Teléfono], " +
                     "m.Titulo AS [Título], p.FechaPrestamo AS Prestado " +
                     "FROM Prestamos p " +
                     "INNER JOIN Usuarios u ON u.Id = p.UsuarioId " +
                     "INNER JOIN Materiales m ON m.Id = p.MaterialId " +
                     "WHERE p.FechaDevolucion IS NULL AND p.FechaVencimiento = CAST(GETDATE() AS DATE) " +
                     "ORDER BY u.Apellidos";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        return await LlenarTabla(cmd);
    }

    public async Task<DataSet> DatosParaRespaldoAsync()
    {
        DataSet datos = new DataSet("SGBU");

        using (SqlCommand cmd = new SqlCommand("SELECT * FROM Materiales ORDER BY Id", conexion, transaccion))
        {
            DataTable tabla = await LlenarTabla(cmd);
            tabla.TableName = "Material";
            datos.Tables.Add(tabla);
        }

        // La contraseña (hash) NO se incluye en el respaldo.
        string sqlUsuarios = "SELECT u.Id, u.Carne, u.Dni, u.Nombre, u.Apellidos, u.Email, u.Telefono, " +
                             "r.Nombre AS Rol, u.Activo, u.FechaRegistro " +
                             "FROM Usuarios u INNER JOIN Roles r ON r.Id = u.RolId ORDER BY u.Id";
        using (SqlCommand cmd = new SqlCommand(sqlUsuarios, conexion, transaccion))
        {
            DataTable tabla = await LlenarTabla(cmd);
            tabla.TableName = "Usuario";
            datos.Tables.Add(tabla);
        }

        using (SqlCommand cmd = new SqlCommand("SELECT * FROM Prestamos ORDER BY Id", conexion, transaccion))
        {
            DataTable tabla = await LlenarTabla(cmd);
            tabla.TableName = "Prestamo";
            datos.Tables.Add(tabla);
        }

        return datos;
    }

    private static async Task<DataTable> LlenarTabla(SqlCommand cmd)
    {
        DataTable tabla = new DataTable();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        tabla.Load(reader);
        return tabla;
    }
}
