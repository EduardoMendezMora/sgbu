using Microsoft.Data.SqlClient;
using SGBU.Domain.Excepciones;

namespace SGBU.Data.Infraestructura;

/// <summary>
/// Convierte los errores de SQL Server en excepciones del dominio con un mensaje que el
/// usuario entiende. Por eso las restricciones de la base tienen nombre: el nombre aparece
/// en el mensaje de SQL Server y aquí se busca para saber qué regla se rompió.
/// La pantalla nunca ve un SqlException ni detalles internos de la base.
/// </summary>
internal static class TraductorErroresSql
{
    private const int ClaveDuplicada = 2627;
    private const int IndiceUnicoDuplicado = 2601;
    private const int RestriccionViolada = 547;

    private static readonly Dictionary<string, string> Mensajes = new(StringComparer.Ordinal)
    {
        ["UQ_Materiales_Isbn"] = "Ya existe un material con ese ISBN.",
        ["UQ_Usuarios_Carne"] = "Ya existe un usuario con ese carné.",
        ["UQ_Usuarios_Dni"] = "Ya existe un usuario con ese DNI.",
        ["UQ_Usuarios_Email"] = "Ya existe un usuario con ese email.",
        ["UQ_Multas_Prestamo"] = "Ese préstamo ya tiene una multa.",
        ["CK_Materiales_Stock"] = "El stock no puede ser negativo ni superar el total.",
        ["CK_Multas_Monto"] = "El monto de la multa debe estar entre ₡0 y ₡2000.",
        ["FK_Usuarios_Roles"] = "El rol indicado no existe.",
        ["FK_Prestamos_Usuarios"] = "El usuario indicado no existe.",
        ["FK_Prestamos_Materiales"] = "El material indicado no existe.",
    };

    public static async Task<T> EjecutarAsync<T>(Func<Task<T>> accion)
    {
        try
        {
            return await accion().ConfigureAwait(false);
        }
        catch (SqlException ex) when (ex.Number is ClaveDuplicada or IndiceUnicoDuplicado or RestriccionViolada)
        {
            throw new DatoInvalidoException(MensajePara(ex.Message));
        }
    }

    public static Task EjecutarAsync(Func<Task> accion) =>
        EjecutarAsync(async () =>
        {
            await accion().ConfigureAwait(false);
            return true;
        });

    private static string MensajePara(string mensajeSql)
    {
        foreach (var (restriccion, mensaje) in Mensajes)
        {
            if (mensajeSql.Contains(restriccion, StringComparison.Ordinal))
            {
                return mensaje;
            }
        }

        return "La base de datos rechazó los datos porque no cumplen una de sus reglas.";
    }
}
