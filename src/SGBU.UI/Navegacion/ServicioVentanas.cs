using System.Windows;
using SGBU.Services.Seguridad;
using SGBU.UI.ViewModels;
using SGBU.UI.Views;

namespace SGBU.UI.Navegacion;

// Abre y cierra las ventanas, para que los ViewModels no tengan que conocer las vistas.
public class ServicioVentanas : IServicioVentanas
{
    private readonly ServicioAutenticacion autenticacion;
    private Window? ventanaActual;

    public ServicioVentanas(ServicioAutenticacion autenticacion)
    {
        this.autenticacion = autenticacion;
    }

    public void MostrarLogin()
    {
        LoginWindow login = new LoginWindow();
        login.DataContext = new LoginViewModel(autenticacion, this);
        Cambiar(login);
    }

    public void MostrarPrincipal(Sesion sesion)
    {
        PrincipalWindow principal = new PrincipalWindow();
        principal.DataContext = new PrincipalViewModel(sesion, this);
        Cambiar(principal);
    }

    public void CerrarSesion()
    {
        MostrarLogin();
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
