namespace SGBU.Domain.Excepciones;

/// <summary>El usuario tiene multas sin pagar, que bloquean nuevos préstamos.</summary>
public sealed class MultasPendientesException : ExcepcionDominio
{
    public MultasPendientesException()
        : base("El usuario tiene multas pendientes. Debe pagarlas antes de un nuevo préstamo.")
    {
    }
}
