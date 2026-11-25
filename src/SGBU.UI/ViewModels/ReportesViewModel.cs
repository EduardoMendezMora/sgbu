using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SGBU.Domain.Excepciones;
using SGBU.Services.Reportes;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

public partial class ReportesViewModel : ObservableObject
{
    private readonly ServicioReportes servicio;
    private readonly Sesion sesion;
    private readonly IServicioVentanas ventanas;
    private DataTable? tablaActual;

    [ObservableProperty]
    private OpcionReporte? reporteSeleccionado;

    [ObservableProperty]
    private DataView? datos;

    [ObservableProperty]
    private bool mostrarGrafico;

    [ObservableProperty]
    private ISeries[] series = new ISeries[0];

    [ObservableProperty]
    private Axis[] ejeX = new Axis[0];

    [ObservableProperty]
    private Axis[] ejeY = new Axis[0];

    [ObservableProperty]
    private string mensaje = string.Empty;

    public ReportesViewModel(ServicioReportes servicio, Sesion sesion, IServicioVentanas ventanas)
    {
        this.servicio = servicio;
        this.sesion = sesion;
        this.ventanas = ventanas;
        PuedeRespaldar = sesion.Puede(Permiso.Configurar);

        Reportes = new List<OpcionReporte>();
        Reportes.Add(new OpcionReporte(TipoReporte.TopMasPrestados));
        Reportes.Add(new OpcionReporte(TipoReporte.UsuariosConMultas));
        Reportes.Add(new OpcionReporte(TipoReporte.StockCritico));
        Reportes.Add(new OpcionReporte(TipoReporte.VencidosHoy));
        ReporteSeleccionado = Reportes[0];
    }

    public List<OpcionReporte> Reportes { get; }

    public bool PuedeRespaldar { get; }

    // Cada vez que se elige otro reporte, se vuelve a consultar.
    partial void OnReporteSeleccionadoChanged(OpcionReporte? value)
    {
        GenerarCommand.Execute(null);
    }

    [RelayCommand]
    private async Task Generar()
    {
        if (ReporteSeleccionado == null)
        {
            return;
        }

        Mensaje = string.Empty;
        try
        {
            tablaActual = await servicio.GenerarAsync(sesion, ReporteSeleccionado.Tipo);
            Datos = tablaActual.DefaultView;
            Mensaje = tablaActual.Rows.Count + " registros.";

            // El gráfico de barras solo se muestra en el Top 10.
            MostrarGrafico = ReporteSeleccionado.Tipo == TipoReporte.TopMasPrestados;
            if (MostrarGrafico)
            {
                ArmarGrafico(tablaActual);
            }
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo generar el reporte. Revise la conexión con la base de datos.";
        }
    }

    [RelayCommand]
    private void ExportarPdf()
    {
        Exportar("Archivo PDF (*.pdf)|*.pdf", ".pdf");
    }

    [RelayCommand]
    private void ExportarExcel()
    {
        Exportar("Archivo de Excel (*.xlsx)|*.xlsx", ".xlsx");
    }

    [RelayCommand]
    private async Task Respaldar()
    {
        string? ruta = ventanas.PedirRutaParaGuardar("Respaldo SGBU " + DateTime.Now.ToString("yyyy-MM-dd") + ".xml", "Archivo XML (*.xml)|*.xml");
        if (ruta == null)
        {
            return;
        }

        try
        {
            await servicio.ExportarRespaldoXmlAsync(sesion, ruta);
            Mensaje = "Respaldo guardado en " + ruta;
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo guardar el respaldo.";
        }
    }

    private void Exportar(string filtro, string extension)
    {
        if (tablaActual == null)
        {
            Mensaje = "Primero genere un reporte.";
            return;
        }

        string? ruta = ventanas.PedirRutaParaGuardar(tablaActual.TableName + extension, filtro);
        if (ruta == null)
        {
            return;
        }

        try
        {
            if (extension == ".pdf")
            {
                Exportador.ExportarPdf(tablaActual, ruta);
            }
            else
            {
                Exportador.ExportarExcel(tablaActual, ruta);
            }

            Mensaje = "Reporte guardado en " + ruta;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo guardar el archivo. ¿Está abierto en otro programa?";
        }
    }

    private void ArmarGrafico(DataTable tabla)
    {
        List<string> titulos = new List<string>();
        List<int> cantidades = new List<int>();
        foreach (DataRow fila in tabla.Rows)
        {
            titulos.Add((string)fila["Título"]);
            cantidades.Add((int)fila["Veces prestado"]);
        }

        ColumnSeries<int> barras = new ColumnSeries<int>();
        barras.Name = "Veces prestado";
        barras.Values = cantidades;
        Series = new ISeries[] { barras };

        Axis eje = new Axis();
        eje.Labels = titulos;
        eje.LabelsRotation = 20;
        EjeX = new Axis[] { eje };

        // El eje vertical empieza en 0 para que las barras se comparen bien.
        Axis ejeVertical = new Axis();
        ejeVertical.MinLimit = 0;
        ejeVertical.MinStep = 1;
        EjeY = new Axis[] { ejeVertical };
    }
}
