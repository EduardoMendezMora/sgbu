using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Auditoria;
using SGBU.Services.Seguridad;

namespace SGBU.UI.ViewModels;

// Bitácora: quién hizo qué y cuándo.
public partial class AuditoriaViewModel : ObservableObject
{
    private readonly ServicioAuditoria servicio;
    private readonly Sesion sesion;
    private int pagina = 1;
    private int totalPaginas = 1;

    [ObservableProperty]
    private string textoBuscar = string.Empty;

    [ObservableProperty]
    private DateTime desde = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime hasta = DateTime.Today;

    [ObservableProperty]
    private string infoPagina = string.Empty;

    [ObservableProperty]
    private string mensaje = string.Empty;

    public AuditoriaViewModel(ServicioAuditoria servicio, Sesion sesion)
    {
        this.servicio = servicio;
        this.sesion = sesion;
        BuscarCommand.Execute(null);
    }

    public ObservableCollection<AuditoriaDetalle> Registros { get; } = new ObservableCollection<AuditoriaDetalle>();

    [RelayCommand]
    private async Task Buscar()
    {
        pagina = 1;
        await CargarPagina();
    }

    [RelayCommand]
    private async Task Anterior()
    {
        if (pagina > 1)
        {
            pagina--;
            await CargarPagina();
        }
    }

    [RelayCommand]
    private async Task Siguiente()
    {
        if (pagina < totalPaginas)
        {
            pagina++;
            await CargarPagina();
        }
    }

    private async Task CargarPagina()
    {
        Mensaje = string.Empty;
        try
        {
            Pagina<AuditoriaDetalle> resultado = await servicio.BuscarAsync(sesion, TextoBuscar, Desde, Hasta, pagina);

            Registros.Clear();
            foreach (AuditoriaDetalle registro in resultado.Elementos)
            {
                Registros.Add(registro);
            }

            totalPaginas = Math.Max(1, resultado.TotalPaginas);
            InfoPagina = pagina + " / " + totalPaginas + " (" + resultado.Total + ")";
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo cargar la auditoría. Revise la conexión con la base de datos.";
        }
    }
}
