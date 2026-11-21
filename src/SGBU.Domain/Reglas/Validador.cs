using System.Text.RegularExpressions;

namespace SGBU.Domain.Reglas;

/// <summary>
/// Validaciones con expresiones regulares que pide el enunciado.
/// [GeneratedRegex] hace que .NET compile la expresión al compilar el proyecto, en vez de
/// interpretarla cada vez que se usa: es más rápido y un error en el patrón se ve al compilar.
/// </summary>
public static partial class Validador
{
    public static bool EsEmailValido(string? email) =>
        !string.IsNullOrWhiteSpace(email) && PatronEmail().IsMatch(email);

    /// <summary>Cédula costarricense (9 dígitos) o DIMEX (11 o 12), con o sin guiones.</summary>
    public static bool EsDniValido(string? dni) =>
        !string.IsNullOrWhiteSpace(dni) && PatronDni().IsMatch(QuitarSeparadores(dni));

    /// <summary>Teléfono de Costa Rica: 8 dígitos que empiezan de 2 a 8, opcionalmente con +506.</summary>
    public static bool EsTelefonoValido(string? telefono) =>
        !string.IsNullOrWhiteSpace(telefono) && PatronTelefono().IsMatch(QuitarSeparadores(telefono));

    private static string QuitarSeparadores(string valor) =>
        valor.Replace("-", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal);

    // algo@algo.algo, sin espacios ni una segunda @
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex PatronEmail();

    [GeneratedRegex(@"^(\d{9}|\d{11,12})$")]
    private static partial Regex PatronDni();

    [GeneratedRegex(@"^(\+506)?[2-8]\d{7}$")]
    private static partial Regex PatronTelefono();
}
