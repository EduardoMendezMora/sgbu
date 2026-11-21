namespace SGBU.Domain.Excepciones;

/// <summary>El préstamo no se puede renovar (ya se renovó, está reservado, vencido o devuelto).</summary>
public sealed class RenovacionNoPermitidaException : ExcepcionDominio
{
    public RenovacionNoPermitidaException(string motivo)
        : base($"No se puede renovar: {motivo}")
    {
    }
}
