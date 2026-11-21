namespace SGBU.Domain.Repositorios;

/// <summary>
/// Crea una unidad de trabajo nueva (una conexión con su transacción) por cada operación.
/// Los servicios piden una, trabajan y la confirman; nunca comparten conexiones entre sí.
/// </summary>
public interface IFabricaUnidadDeTrabajo
{
    Task<IUnidadDeTrabajo> CrearAsync(CancellationToken ct = default);
}
