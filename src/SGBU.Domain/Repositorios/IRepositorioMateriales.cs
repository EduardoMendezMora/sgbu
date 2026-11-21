using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

/// <summary>
/// Contrato del acceso a datos de Materiales. Domain solo define QUÉ se necesita;
/// SGBU.Data decide CÓMO (ADO.NET contra SQL Server). Así las reglas no dependen de la base
/// y en las pruebas se puede usar una implementación falsa en memoria.
/// </summary>
public interface IRepositorioMateriales
{
    Task<Material?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    Task<Material?> ObtenerPorIsbnAsync(string isbn, CancellationToken ct = default);

    Task<Pagina<Material>> BuscarAsync(string? texto, TipoMaterial? tipo, int numeroPagina, int tamanoPagina, CancellationToken ct = default);

    Task<int> AgregarAsync(Material material, CancellationToken ct = default);

    Task ActualizarAsync(Material material, CancellationToken ct = default);
}
