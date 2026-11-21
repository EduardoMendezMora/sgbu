namespace SGBU.Domain.Repositorios;

// Resultado de una búsqueda paginada: los elementos de una página y el total de registros.
public class Pagina<T>
{
    public Pagina(IReadOnlyList<T> elementos, int numeroPagina, int tamanoPagina, int total)
    {
        Elementos = elementos;
        NumeroPagina = numeroPagina;
        TamanoPagina = tamanoPagina;
        Total = total;
    }

    public IReadOnlyList<T> Elementos { get; }

    public int NumeroPagina { get; }

    public int TamanoPagina { get; }

    public int Total { get; }

    public int TotalPaginas
    {
        get
        {
            if (TamanoPagina <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(Total / (double)TamanoPagina);
        }
    }

    public bool HayAnterior => NumeroPagina > 1;

    public bool HaySiguiente => NumeroPagina < TotalPaginas;
}
