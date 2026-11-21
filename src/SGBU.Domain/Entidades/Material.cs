using SGBU.Domain.Excepciones;
using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

/// <summary>
/// Clase base de Libro, Revista y Tesis (herencia que pide el enunciado).
/// Es abstracta: no existe "un material" a secas, siempre es de un tipo concreto.
///
/// Encapsulamiento: el stock tiene "private set". Nadie de afuera puede escribir
/// material.StockDisponible = -5; solo se cambia con Prestar() y Devolver(), que revisan las reglas.
/// </summary>
public abstract class Material
{
    protected Material(int id, string isbn, string titulo, string autor, int stockTotal, int stockDisponible)
    {
        if (stockTotal < 0 || stockDisponible < 0 || stockDisponible > stockTotal)
        {
            throw new DatoInvalidoException("El stock no puede ser negativo y lo disponible no puede superar el total.");
        }

        Id = id;
        Isbn = Reglas.Isbn.Normalizar(isbn);
        Titulo = Requerir.Texto(titulo, "El título");
        Autor = Requerir.Texto(autor, "El autor");
        StockTotal = stockTotal;
        StockDisponible = stockDisponible;
    }

    public int Id { get; }

    /// <summary>Cada clase hija dice qué tipo es; se guarda en la columna Tipo.</summary>
    public abstract TipoMaterial Tipo { get; }

    public string Isbn { get; }

    public string Titulo { get; }

    public string Autor { get; }

    public string? Editorial { get; init; }

    public short? AnioPublicacion { get; init; }

    public string? Categoria { get; init; }

    public bool Activo { get; init; } = true;

    public int StockTotal { get; }

    public int StockDisponible { get; private set; }

    public bool EsStockCritico => StockDisponible <= ReglasBiblioteca.StockCritico;

    /// <summary>Polimorfismo: cada tipo arma su propia descripción (la usa el catálogo en la UI).</summary>
    public abstract string Detalle { get; }

    public void Prestar()
    {
        if (StockDisponible == 0)
        {
            throw new StockInsuficienteException(Titulo);
        }

        StockDisponible--;
    }

    public void Devolver()
    {
        if (StockDisponible == StockTotal)
        {
            throw new OperacionNoPermitidaException($"Todos los ejemplares de \"{Titulo}\" ya están en la biblioteca.");
        }

        StockDisponible++;
    }
}
