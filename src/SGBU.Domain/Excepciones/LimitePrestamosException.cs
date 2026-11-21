namespace SGBU.Domain.Excepciones;

/// <summary>El usuario ya tiene el máximo de préstamos activos que permite su rol.</summary>
public sealed class LimitePrestamosException : ExcepcionDominio
{
    public LimitePrestamosException(string rol, int limite)
        : base($"El rol {rol} permite máximo {limite} préstamos activos.")
    {
    }
}
