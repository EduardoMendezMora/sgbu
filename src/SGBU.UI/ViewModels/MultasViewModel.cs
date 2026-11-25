using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Multas;
using SGBU.Services.Seguridad;

namespace SGBU.UI.ViewModels;

// Pantalla del administrador: multas pendientes y registro de pagos.
public partial class MultasViewModel : ObservableObject
{
    private readonly ServicioMultas servicio;
    private readonly Sesion sesion;

    [ObservableProperty]
    private string textoBuscar = string.Empty;

    [ObservableProperty]
    private MultaDetalle? multaSeleccionada;

    [ObservableProperty]
    private string total = string.Empty;

    [ObservableProperty]
    private string mensaje = string.Empty;

    public MultasViewModel(ServicioMultas servicio, Sesion sesion)
    {
        this.servicio = servicio;
        this.sesion = sesion;
        BuscarCommand.Execute(null);
    }

    public ObservableCollection<MultaDetalle> Multas { get; } = new ObservableCollection<MultaDetalle>();

    [RelayCommand]
    private async Task Buscar()
    {
        try
        {
            IReadOnlyList<MultaDetalle> lista = await servicio.ListarPendientesAsync(sesion, TextoBuscar);
            Multas.Clear();
            foreach (MultaDetalle multa in lista)
            {
                Multas.Add(multa);
            }

            Total = "Total pendiente: ₡" + ServicioMultas.Total(lista).ToString("N0") + " (" + lista.Count + " multas)";
        }
        catch (Exception)
        {
            Mensaje = "No se pudo cargar la lista de multas.";
        }
    }

    [RelayCommand]
    private async Task Pagar()
    {
        if (MultaSeleccionada == null)
        {
            Mensaje = "Seleccione una multa de la tabla.";
            return;
        }

        try
        {
            string nombre = MultaSeleccionada.Usuario;
            decimal monto = MultaSeleccionada.Monto;
            await servicio.PagarAsync(sesion, MultaSeleccionada.Id);
            Mensaje = "Pago de ₡" + monto.ToString("N0") + " registrado para " + nombre + ".";
            await Buscar();
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo registrar el pago. Revise la conexión con la base de datos.";
        }
    }
}
