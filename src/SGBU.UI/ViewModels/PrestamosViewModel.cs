using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Prestamos;
using SGBU.Services.Seguridad;

namespace SGBU.UI.ViewModels;

// Pantalla del bibliotecario: prestar, devolver y renovar.
public partial class PrestamosViewModel : ObservableObject
{
    private readonly ServicioPrestamos servicio;
    private readonly Sesion sesion;

    [ObservableProperty]
    private string carne = string.Empty;

    [ObservableProperty]
    private string isbn = string.Empty;

    [ObservableProperty]
    private string textoBuscar = string.Empty;

    [ObservableProperty]
    private PrestamoDetalle? prestamoSeleccionado;

    [ObservableProperty]
    private string mensaje = string.Empty;

    public PrestamosViewModel(ServicioPrestamos servicio, Sesion sesion)
    {
        this.servicio = servicio;
        this.sesion = sesion;
        BuscarCommand.Execute(null);
    }

    public ObservableCollection<PrestamoDetalle> Prestamos { get; } = new ObservableCollection<PrestamoDetalle>();

    [RelayCommand]
    private async Task Prestar()
    {
        try
        {
            Prestamo prestamo = await servicio.RegistrarAsync(sesion, Carne, Isbn);
            Mensaje = "Préstamo registrado. Vence el " + prestamo.FechaVencimiento.ToString("dd/MM/yyyy") + ".";
            Carne = string.Empty;
            Isbn = string.Empty;
            await Cargar();
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo registrar el préstamo. Revise la conexión con la base de datos.";
        }
    }

    [RelayCommand]
    private async Task Devolver()
    {
        if (PrestamoSeleccionado == null)
        {
            Mensaje = "Seleccione un préstamo de la tabla.";
            return;
        }

        try
        {
            ResultadoDevolucion resultado = await servicio.DevolverAsync(sesion, PrestamoSeleccionado.Id);
            if (resultado.DiasAtraso > 0)
            {
                Mensaje = "Devuelto con " + resultado.DiasAtraso + " días de atraso. Se generó una multa de ₡" + resultado.Monto.ToString("N0") + ".";
            }
            else
            {
                Mensaje = "Devuelto a tiempo.";
            }

            await Cargar();
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo registrar la devolución. Revise la conexión con la base de datos.";
        }
    }

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
    private async Task Buscar()
    {
        await Cargar();
    }

    private async Task Cargar()
    {
        try
        {
            IReadOnlyList<PrestamoDetalle> lista = await servicio.ListarActivosAsync(sesion, TextoBuscar);
            Prestamos.Clear();
            foreach (PrestamoDetalle prestamo in lista)
            {
                Prestamos.Add(prestamo);
            }
        }
        catch (Exception)
        {
            Mensaje = "No se pudo cargar la lista de préstamos.";
        }
    }
}
