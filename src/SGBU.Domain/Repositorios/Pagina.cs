namespace SGBU.Domain.Repositorios;

/// <summary>
/// Resultado de una consulta paginada del lado del servidor: SQL Server devuelve solo la
/// página pedida (OFFSET/FETCH) y el total, en vez de traer miles de filas a la aplicación.
/// </summary>
public sealed record Pagina<T>(IReadOnlyList<T> Elementos, int NumeroPagina, int TamanoPagina, int Total)
{
    public int TotalPaginas => TamanoPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanoPagina);

    public bool HayAnterior => NumeroPagina > 1;

    public bool HaySiguiente => NumeroPagina < TotalPaginas;
}
