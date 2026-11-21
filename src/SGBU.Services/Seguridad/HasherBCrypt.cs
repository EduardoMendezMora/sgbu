namespace SGBU.Services.Seguridad;

/// <summary>
/// BCrypt: la contraseña nunca se guarda, solo su hash. Por qué BCrypt y no SHA-256:
/// - Es lento a propósito (factor de trabajo 11 = 2^11 rondas): probar millones de claves
///   robadas tomaría años.
/// - Cada hash lleva una "sal" aleatoria: dos usuarios con la misma clave tienen hashes
///   distintos, y no sirven las tablas de hashes precalculados.
/// </summary>
public sealed class HasherBCrypt : IHasherContrasenas
{
    private const int FactorDeTrabajo = 11;

    public string Generar(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena, FactorDeTrabajo);

    public bool Verificar(string contrasena, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Un hash dañado en la base no debe tumbar el login: simplemente no coincide.
            return false;
        }
    }
}
