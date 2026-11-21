namespace SGBU.Domain.Excepciones;

/// <summary>
/// Base de todas las excepciones de reglas de negocio. La UI puede capturar solo esta para
/// mostrar el mensaje al usuario, y dejar que los errores técnicos (base caída, etc.) se
/// manejen aparte.
/// </summary>
public abstract class ExcepcionDominio : Exception
{
    protected ExcepcionDominio(string mensaje)
        : base(mensaje)
    {
    }
}
