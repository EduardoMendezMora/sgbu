using SGBU.Domain.Repositorios;

namespace SGBU.Data;

// Guarda la cadena de conexión y crea una unidad de trabajo nueva cada vez que se pide.
public class FabricaUnidadDeTrabajoSql : IFabricaUnidadDeTrabajo
{
    private readonly string cadenaConexion;

    public FabricaUnidadDeTrabajoSql(string cadenaConexion)
    {
        this.cadenaConexion = cadenaConexion;
    }

    public async Task<IUnidadDeTrabajo> CrearAsync()
    {
        return await UnidadDeTrabajoSql.AbrirAsync(cadenaConexion);
    }
}
