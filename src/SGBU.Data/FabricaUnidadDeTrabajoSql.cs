using SGBU.Domain.Repositorios;

namespace SGBU.Data;

/// <summary>
/// La UI la registra una sola vez con la cadena de conexión (leída de la configuración,
/// nunca escrita en el código) y los servicios la usan para abrir una unidad de trabajo
/// por operación.
/// </summary>
public sealed class FabricaUnidadDeTrabajoSql : IFabricaUnidadDeTrabajo
{
    private readonly string cadenaConexion;

    public FabricaUnidadDeTrabajoSql(string cadenaConexion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);
        this.cadenaConexion = cadenaConexion;
    }

    public async Task<IUnidadDeTrabajo> CrearAsync(CancellationToken ct = default) =>
        await UnidadDeTrabajoSql.AbrirAsync(cadenaConexion, ct);
}
