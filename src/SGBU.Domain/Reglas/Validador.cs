using System.Text.RegularExpressions;

namespace SGBU.Domain.Reglas;

// Validaciones con expresiones regulares (Regex) que pide el enunciado.
public static class Validador
{
    // algo@algo.algo, sin espacios
    private const string PatronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    // Cédula de 9 dígitos o DIMEX de 11 o 12 dígitos
    private const string PatronDni = @"^(\d{9}|\d{11,12})$";

    // Teléfono de Costa Rica: 8 dígitos que empiezan de 2 a 8, con +506 opcional
    private const string PatronTelefono = @"^(\+506)?[2-8]\d{7}$";

    public static bool EsEmailValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return Regex.IsMatch(email, PatronEmail);
    }

    public static bool EsDniValido(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
        {
            return false;
        }

        return Regex.IsMatch(QuitarSeparadores(dni), PatronDni);
    }

    public static bool EsTelefonoValido(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            return false;
        }

        return Regex.IsMatch(QuitarSeparadores(telefono), PatronTelefono);
    }

    // Quita guiones y espacios: "8888-0001" queda "88880001".
    private static string QuitarSeparadores(string valor)
    {
        return valor.Replace("-", string.Empty).Replace(" ", string.Empty);
    }
}
