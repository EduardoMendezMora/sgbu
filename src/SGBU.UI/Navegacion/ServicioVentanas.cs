using System.Windows;
using Microsoft.Win32;
using SGBU.Services.Seguridad;
using SGBU.UI.ViewModels;
using SGBU.UI.Views;

namespace SGBU.UI.Navegacion;

// Abre y cierra las ventanas, para que los ViewModels no tengan que conocer las vistas.
public class ServicioVentanas : IServicioVentanas
{
    private readonly Servicios servicios;
    private Window? ventanaActual;

    public ServicioVentanas(Servicios servicios)
    {
        this.servicios = servicios;
    }

    public void MostrarLogin()
    {
        LoginWindow login = new LoginWindow();
        login.DataContext = new LoginViewModel(servicios.Autenticacion, this);
        Cambiar(login);
    }

    public void MostrarPrincipal(Sesion sesion)
    {
        PrincipalWindow principal = new PrincipalWindow();
        principal.DataContext = new PrincipalViewModel(sesion, this, servicios);
        Cambiar(principal);
    }

    public void CerrarSesion()
    {
        MostrarLogin();
    }

    public string? PedirRutaParaGuardar(string nombreSugerido, string filtro)
    {
        SaveFileDialog dialogo = new SaveFileDialog();
        dialogo.FileName = nombreSugerido;
        dialogo.Filter = filtro;

        if (dialogo.ShowDialog() == true)
        {
            return dialogo.FileName;
        }

        return null;
    }

    // Muestra la ventana nueva y cierra la anterior.
    private void Cambiar(Window nueva)
    {
        Window? anterior = ventanaActual;
        ventanaActual = nueva;
        nueva.Closed += AlCerrarVentana;
        nueva.Show();

        if (anterior != null)
        {
            anterior.Closed -= AlCerrarVentana;
            anterior.Close();
        }
    }

    // Si el usuario cierra la ventana con la X, termina el programa.
    private void AlCerrarVentana(object? sender, EventArgs e)
    {
        Application.Current.Shutdown();
    }
}
