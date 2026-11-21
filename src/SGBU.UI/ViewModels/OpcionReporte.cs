using SGBU.Services.Reportes;

namespace SGBU.UI.ViewModels;

// Un reporte de la lista desplegable.
public class OpcionReporte
{
    public OpcionReporte(TipoReporte tipo)
    {
        Tipo = tipo;
        Titulo = ServicioReportes.Titulo(tipo);
    }

    public TipoReporte Tipo { get; }

    public string Titulo { get; }

    public override string ToString()
    {
        return Titulo;
    }
}
