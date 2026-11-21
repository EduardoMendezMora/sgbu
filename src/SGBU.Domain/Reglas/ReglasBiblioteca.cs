namespace SGBU.Domain.Reglas;

/// <summary>
/// Números del enunciado en un solo lugar. Si el profe cambia la multa o el tope, se cambia aquí.
/// </summary>
public static class ReglasBiblioteca
{
    public const decimal MultaPorDia = 50m;

    public const decimal TopeMulta = 2000m;

    public const int StockCritico = 3;
}
