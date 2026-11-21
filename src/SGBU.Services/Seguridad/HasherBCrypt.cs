namespace SGBU.Services.Seguridad;

// La contraseña nunca se guarda: se guarda su hash BCrypt.
// BCrypt es lento a propósito y le agrega una "sal" aleatoria a cada hash.
public class HasherBCrypt : IHasherContrasenas
{
    public string Generar(string contrasena)
    {
        return BCrypt.Net.BCrypt.HashPassword(contrasena, 11);
    }

    public bool Verificar(string contrasena, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Si el hash guardado está dañado, se toma como contraseña incorrecta.
            return false;
        }
    }
}
