using Microsoft.Data.SqlClient;

namespace SGBU.Data.Infraestructura;

/// <summary>
/// La conexión y la transacción que comparten todos los repositorios de una misma unidad
/// de trabajo. Cada comando sale de aquí, así ninguno se ejecuta fuera de la transacción.
/// </summary>
internal sealed class ContextoSql
{
    private const int SegundosDeEspera = 30;

    public ContextoSql(SqlConnection conexion, SqlTransaction transaccion)
    {
        Conexion = conexion;
        Transaccion = transaccion;
    }

    public SqlConnection Conexion { get; }

    public SqlTransaction? Transaccion { get; set; }

    public SqlCommand Comando(string sql)
    {
        var transaccion = Transaccion
            ?? throw new InvalidOperationException("La unidad de trabajo ya se confirmó o se revirtió; abra otra.");

        return new SqlCommand(sql, Conexion, transaccion) { CommandTimeout = SegundosDeEspera };
    }
}
