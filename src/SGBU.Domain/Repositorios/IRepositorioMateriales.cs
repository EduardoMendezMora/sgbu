using SGBU.Domain.Entidades;

namespace SGBU.Domain.Repositorios;

/// <summary>
/// Contrato del acceso a datos de Materiales. Domain solo define QUÉ se necesita;
/// SGBU.Data decide CÓMO (ADO.NET contra SQL Server). Así las reglas no dependen de la base
/// y en las pruebas se puede usar una implementación falsa en memoria.
/// </summary>
public interface IRepositorioMateriales
{
    Task<Material?> ObtenerPorIdAsync(int id);

    Task<Material?> ObtenerPorIsbnAsync(string isbn);

    Task<Pagina<Material>> BuscarAsync(string? texto, TipoMaterial? tipo, int numeroPagina, int tamanoPagina);

    Task<int> AgregarAsync(Material material);

    Task ActualizarAsync(Material material);
}
