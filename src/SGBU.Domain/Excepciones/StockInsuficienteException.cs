namespace SGBU.Domain.Excepciones;

/// <summary>No quedan ejemplares disponibles para prestar.</summary>
public class StockInsuficienteException : ExcepcionDominio
{
    public StockInsuficienteException(string titulo)
        : base($"No hay ejemplares disponibles de \"{titulo}\".")
    {
    }
}
