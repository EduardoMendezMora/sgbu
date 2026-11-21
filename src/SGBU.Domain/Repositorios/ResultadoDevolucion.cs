namespace SGBU.Domain.Repositorios;

// Lo que devuelve sp_RegistrarDevolucion.
public class ResultadoDevolucion
{
    public ResultadoDevolucion(int diasAtraso, decimal monto)
    {
        DiasAtraso = diasAtraso;
        Monto = monto;
    }

    public int DiasAtraso { get; }

    public decimal Monto { get; }
}
