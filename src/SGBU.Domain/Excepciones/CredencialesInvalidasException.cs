namespace SGBU.Domain.Excepciones;

/// <summary>
/// Usuario o contraseña incorrectos. El mensaje es el mismo en los dos casos a propósito:
/// si dijera "ese correo no existe", cualquiera podría averiguar quién tiene cuenta.
/// </summary>
public sealed class CredencialesInvalidasException : ExcepcionDominio
{
    public CredencialesInvalidasException()
        : base("Usuario o contraseña incorrectos.")
    {
    }
}
