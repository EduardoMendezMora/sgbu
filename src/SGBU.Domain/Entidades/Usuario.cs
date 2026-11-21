using SGBU.Domain.Excepciones;
using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

/// <summary>
/// Persona que usa la biblioteca. Valida email, DNI y teléfono al crearse: un Usuario con
/// datos inválidos no puede existir en memoria, ni hablar de llegar a la base.
/// </summary>
public class Usuario
{
    private const int LargoHashBCrypt = 60;

    public Usuario(string carne, string dni, string nombre, string apellidos, string email, string? telefono, Rol rol, string passwordHash)
    {
        if (!Validador.EsEmailValido(email))
        {
            throw new DatoInvalidoException($"El email \"{email}\" no es válido.");
        }

        if (!Validador.EsDniValido(dni))
        {
            throw new DatoInvalidoException($"El DNI \"{dni}\" no es una cédula (9 dígitos) ni un DIMEX (11 o 12).");
        }

        if (telefono is not null && !Validador.EsTelefonoValido(telefono))
        {
            throw new DatoInvalidoException($"El teléfono \"{telefono}\" no es un número de Costa Rica válido.");
        }

        // Defensa extra: si alguien por error manda la contraseña en texto plano, se rechaza.
        if (passwordHash is null || passwordHash.Length != LargoHashBCrypt || !passwordHash.StartsWith("$2", StringComparison.Ordinal))
        {
            throw new DatoInvalidoException("La contraseña debe llegar como hash BCrypt, nunca en texto plano.");
        }

        Carne = Requerir.Texto(carne, "El carné");
        Dni = dni.Trim();
        Nombre = Requerir.Texto(nombre, "El nombre");
        Apellidos = Requerir.Texto(apellidos, "Los apellidos");
        Email = email.Trim();
        Telefono = telefono?.Trim();
        Rol = rol ?? throw new DatoInvalidoException("El rol es obligatorio.");
        PasswordHash = passwordHash;
    }

    public int Id { get; init; }

    public string Carne { get; }

    public string Dni { get; }

    public string Nombre { get; }

    public string Apellidos { get; }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public string Email { get; }

    public string? Telefono { get; }

    public Rol Rol { get; }

    public string PasswordHash { get; }

    public bool Activo { get; private set; } = true;

    public DateTime FechaRegistro { get; init; }

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;

    /// <summary>
    /// Reglas para pedir un préstamo. Los números (cuántos préstamos activos, si hay multas)
    /// los consulta Services en la base y se los pasa aquí: Domain decide, pero no consulta.
    /// </summary>
    public void ValidarPuedePrestar(int prestamosActivos, bool tieneMultasPendientes)
    {
        if (!Activo)
        {
            throw new OperacionNoPermitidaException("El usuario está inactivo.");
        }

        if (tieneMultasPendientes)
        {
            throw new MultasPendientesException();
        }

        if (prestamosActivos >= Rol.LimitePrestamos)
        {
            throw new LimitePrestamosException(Rol.Nombre, Rol.LimitePrestamos);
        }
    }
}
