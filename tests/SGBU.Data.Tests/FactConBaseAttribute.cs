using Xunit;

namespace SGBU.Data.Tests;

/// <summary>
/// Igual que [Fact], pero se omite (en vez de fallar) si no hay base de datos configurada.
/// </summary>
public class FactConBaseAttribute : FactAttribute
{
    public FactConBaseAttribute()
    {
        if (string.IsNullOrWhiteSpace(BaseDeDatos.Cadena))
        {
            Skip = "Sin SGBU_CONN: no hay SQL Server para pruebas de integración.";
        }
    }
}
