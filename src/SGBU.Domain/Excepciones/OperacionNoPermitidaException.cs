namespace SGBU.Domain.Excepciones;

/// <summary>La operación no tiene sentido en el estado actual (devolver dos veces, pagar una multa pagada...).</summary>
public sealed class OperacionNoPermitidaException : ExcepcionDominio
{
    public OperacionNoPermitidaException(string mensaje)
        : base(mensaje)
    {
    }
}
