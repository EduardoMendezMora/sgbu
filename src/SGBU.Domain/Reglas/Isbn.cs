using SGBU.Domain.Excepciones;

namespace SGBU.Domain.Reglas;

/// <summary>
/// Validación del ISBN-13.
/// El último dígito es un "dígito verificador": se multiplican los 12 primeros por 1, 3, 1, 3...,
/// se suman, y el verificador es lo que falta para llegar a la siguiente decena.
/// Así se detecta casi cualquier error de digitación.
/// </summary>
public static class Isbn
{
    private const int Largo = 13;

    public static bool EsValido(string? valor)
    {
        var digitos = QuitarSeparadores(valor);
        if (digitos.Length != Largo || !digitos.All(char.IsAsciiDigit))
        {
            return false;
        }

        var suma = 0;
        for (var i = 0; i < Largo - 1; i++)
        {
            var digito = digitos[i] - '0';
            suma += i % 2 == 0 ? digito : digito * 3;
        }

        var verificador = (10 - (suma % 10)) % 10;
        return verificador == digitos[Largo - 1] - '0';
    }

    /// <summary>Devuelve los 13 dígitos sin guiones ni espacios, o lanza excepción si no es válido.</summary>
    public static string Normalizar(string? valor)
    {
        if (!EsValido(valor))
        {
            throw new DatoInvalidoException($"El ISBN \"{valor}\" no es un ISBN-13 válido.");
        }

        return QuitarSeparadores(valor);
    }

    private static string QuitarSeparadores(string? valor) =>
        (valor ?? string.Empty).Replace("-", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal);
}
