namespace SGBU.Tests.Services;

internal sealed class RelojFijo(DateTime ahora) : SGBU.Services.Comun.IReloj
{
    public DateTime Ahora { get; } = ahora;
}
