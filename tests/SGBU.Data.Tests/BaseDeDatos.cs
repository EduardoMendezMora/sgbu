using SGBU.Data;

namespace SGBU.Data.Tests;

/// <summary>
/// Punto único para conectarse a la base de pruebas. La cadena de conexión viene de la
/// variable de entorno SGBU_CONN, nunca del código (así no se sube una contraseña a GitHub).
/// </summary>
internal static class BaseDeDatos
{
    public static string? Cadena => Environment.GetEnvironmentVariable("SGBU_CONN");

    /// <summary>
    /// Cada prueba trabaja dentro de una transacción que al final se revierte
    /// (DisposeAsync sin ConfirmarAsync). Así las pruebas nunca ensucian la base
    /// y se pueden correr mil veces con el mismo resultado.
    /// </summary>
    public static Task<UnidadDeTrabajoSql> AbrirAsync() =>
        UnidadDeTrabajoSql.AbrirAsync(Cadena ?? throw new InvalidOperationException("Falta SGBU_CONN."));
}
