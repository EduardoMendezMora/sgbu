using System.Data;
using Microsoft.Data.SqlClient;

namespace SGBU.Data.Infraestructura;

/// <summary>
/// Ayudantes para no repetir código en cada repositorio.
/// Los parámetros se crean SIEMPRE con su tipo SQL explícito (nunca AddWithValue):
/// así SQL Server no tiene que adivinar el tipo y reutiliza el plan de ejecución,
/// y los valores nunca se pegan dentro del texto SQL (eso evita la inyección SQL).
/// </summary>
internal static class ExtensionesSql
{
    public static SqlCommand Con(this SqlCommand comando, string nombre, SqlDbType tipo, object? valor, int tamano = 0)
    {
        var parametro = tamano > 0 ? new SqlParameter(nombre, tipo, tamano) : new SqlParameter(nombre, tipo);
        parametro.Value = valor ?? DBNull.Value;
        comando.Parameters.Add(parametro);
        return comando;
    }

    public static SqlCommand ConFecha(this SqlCommand comando, string nombre, DateOnly fecha) =>
        comando.Con(nombre, SqlDbType.Date, fecha.ToDateTime(TimeOnly.MinValue));

    /// <summary>Para buscar con LIKE: los comodines que escribe el usuario se toman como texto.</summary>
    public static string? PatronContiene(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        var escapado = texto.Trim()
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal);
        return $"%{escapado}%";
    }

    public static int Saltar(int numeroPagina, int tamanoPagina)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numeroPagina, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(tamanoPagina, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tamanoPagina, 100);
        return (numeroPagina - 1) * tamanoPagina;
    }

    public static string Texto(this SqlDataReader r, string columna) => r.GetString(r.GetOrdinal(columna));

    public static string? TextoONulo(this SqlDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetString(i);
    }

    public static int Entero(this SqlDataReader r, string columna) => r.GetInt32(r.GetOrdinal(columna));

    public static int? EnteroONulo(this SqlDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetInt32(i);
    }

    public static short? CortoONulo(this SqlDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetInt16(i);
    }

    public static bool Booleano(this SqlDataReader r, string columna) => r.GetBoolean(r.GetOrdinal(columna));

    public static decimal Decimal(this SqlDataReader r, string columna) => r.GetDecimal(r.GetOrdinal(columna));

    public static DateTime Fecha(this SqlDataReader r, string columna) => r.GetDateTime(r.GetOrdinal(columna));

    public static DateTime? FechaONula(this SqlDataReader r, string columna)
    {
        var i = r.GetOrdinal(columna);
        return r.IsDBNull(i) ? null : r.GetDateTime(i);
    }

    public static DateOnly SoloFecha(this SqlDataReader r, string columna) => DateOnly.FromDateTime(r.Fecha(columna));
}
