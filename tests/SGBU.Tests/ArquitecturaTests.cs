using System.Reflection;
using Xunit;

namespace SGBU.Tests;

/// <summary>
/// Verifica las reglas de Clean Architecture: las dependencias solo apuntan hacia Domain.
/// </summary>
public class ArquitecturaTests
{
    [Fact]
    public void Domain_NoDependeDeNingunaOtraCapa()
    {
        var referencias = ReferenciasSgbu(typeof(SGBU.Domain.AssemblyReference).Assembly);

        Assert.Empty(referencias);
    }

    [Fact]
    public void Services_NoDependeDeDataNiDeUI()
    {
        var referencias = ReferenciasSgbu(typeof(SGBU.Services.AssemblyReference).Assembly);

        Assert.DoesNotContain("SGBU.Data", referencias);
        Assert.DoesNotContain("SGBU.UI", referencias);
    }

    private static List<string> ReferenciasSgbu(Assembly ensamblado)
    {
        return ensamblado
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(nombre => nombre.StartsWith("SGBU.", StringComparison.Ordinal))
            .ToList();
    }
}
