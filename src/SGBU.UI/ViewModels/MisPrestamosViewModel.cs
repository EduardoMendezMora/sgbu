using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Multas;
using SGBU.Services.Prestamos;
using SGBU.Services.Seguridad;

namespace SGBU.UI.ViewModels;

// Pantalla del estudiante o profesor: ve sus préstamos y puede renovarlos.
public partial class MisPrestamosViewModel : ObservableObject
{
    private readonly ServicioPrestamos servicio;
    private readonly ServicioMultas servicioMultas;
    private readonly Sesion sesion;

    [ObservableProperty]
    private string totalMultas = string.Empty;

    [ObservableProperty]
    private PrestamoDetalle? prestamoSeleccionado;

    [ObservableProperty]
    private string mensaje = string.Empty;

    public MisPrestamosViewModel(ServicioPrestamos servicio, ServicioMultas servicioMultas, Sesion sesion)
    {
        this.servicio = servicio;
        this.servicioMultas = servicioMultas;
        this.sesion = sesion;
        CargarCommand.Execute(null);
    }

    public ObservableCollection<PrestamoDetalle> Prestamos { get; } = new ObservableCollection<PrestamoDetalle>();

    public ObservableCollection<MultaDetalle> Multas { get; } = new ObservableCollection<MultaDetalle>();

    [RelayCommand]
    private async Task Renovar()
    {
        if (PrestamoSeleccionado == null)
        {
            Mensaje = "Seleccione un préstamo de la tabla.";
            return;
        }

        try
        {
            await servicio.RenovarAsync(sesion, PrestamoSeleccionado.Id);
            Mensaje = "Préstamo renovado.";
            await Cargar();
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo renovar. Revise la conexión con la base de datos.";
        }
    }

    [RelayCommand]
    private async Task Cargar()
    {
        try
        {
            IReadOnlyList<PrestamoDetalle> lista = await servicio.ListarMisPrestamosAsync(sesion);
            Prestamos.Clear();
            foreach (PrestamoDetalle prestamo in lista)
            {
                Prestamos.Add(prestamo);
            }

            if (lista.Count == 0)
            {
                Mensaje = "No tiene préstamos activos.";
            }

            IReadOnlyList<MultaDetalle> multas = await servicioMultas.ListarMisMultasAsync(sesion);
            Multas.Clear();
            foreach (MultaDetalle multa in multas)
            {
                Multas.Add(multa);
            }

            if (multas.Count == 0)
            {
                TotalMultas = "No tiene multas pendientes.";
            }
            else
            {
                TotalMultas = "Debe ₡" + ServicioMultas.Total(multas).ToString("N0") + ". Mientras tenga multas no puede pedir préstamos.";
            }
        }
        catch (Exception)
        {
            Mensaje = "No se pudo cargar la lista de préstamos.";
        }
    }
}
