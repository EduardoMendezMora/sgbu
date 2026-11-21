namespace SGBU.Domain.Excepciones;

/// <summary>Un dato no cumple el formato o las reglas (ISBN, email, stock negativo...).</summary>
public sealed class DatoInvalidoException : ExcepcionDominio
{
    public DatoInvalidoException(string mensaje)
        : base(mensaje)
    {
    }
}
