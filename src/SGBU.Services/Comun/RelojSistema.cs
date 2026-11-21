namespace SGBU.Services.Comun;

public sealed class RelojSistema : IReloj
{
    public DateTime Ahora => DateTime.Now;
}
