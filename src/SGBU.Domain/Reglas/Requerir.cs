using SGBU.Domain.Excepciones;

namespace SGBU.Domain.Reglas;

/// <summary>Validaciones repetidas de los constructores, para no copiar el mismo if en cada entidad.</summary>
internal static class Requerir
{
    public static string Texto(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DatoInvalidoException($"{campo} es obligatorio.");
        }

        return valor.Trim();
    }

    public static string? TextoOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    public static int Positivo(int valor, string campo)
    {
        if (valor <= 0)
        {
            throw new DatoInvalidoException($"{campo} debe ser mayor que cero.");
        }

        return valor;
    }
}
