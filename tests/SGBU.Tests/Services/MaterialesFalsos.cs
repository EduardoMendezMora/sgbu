using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class MaterialesFalsos : IRepositorioMateriales
{
    private readonly List<Material> datos;

    public MaterialesFalsos(List<Material> datos) => this.datos = datos;

    public Task<Material?> ObtenerPorIdAsync(int id) => Task.FromResult(datos.FirstOrDefault(m => m.Id == id));

    public Task<Material?> ObtenerPorIsbnAsync(string isbn) => Task.FromResult(datos.FirstOrDefault(m => m.Isbn == isbn));

    public Task<Pagina<Material>> BuscarAsync(string? texto, TipoMaterial? tipo, int numeroPagina, int tamanoPagina)
    {
        var filtrados = datos
            .Where(m => texto == null || m.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase))
            .Where(m => tipo == null || m.Tipo == tipo)
            .ToList();
        var pagina = filtrados.Skip((numeroPagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();
        return Task.FromResult(new Pagina<Material>(pagina, numeroPagina, tamanoPagina, filtrados.Count));
    }

    public Task<int> AgregarAsync(Material material)
    {
        datos.Add(material);
        return Task.FromResult(datos.Count + 100);
    }

    public Task ActualizarAsync(Material material)
    {
        datos.RemoveAll(m => m.Id == material.Id);
        datos.Add(material);
        return Task.CompletedTask;
    }
}
