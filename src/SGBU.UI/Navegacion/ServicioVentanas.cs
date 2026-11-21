using System.Windows;
using Microsoft.Win32;
using SGBU.Services.Catalogo;
using SGBU.Services.Multas;
using SGBU.Services.Prestamos;
using SGBU.Services.Reportes;
using SGBU.Services.Seguridad;
using SGBU.Services.Usuarios;
using SGBU.UI.ViewModels;
using SGBU.UI.Views;

namespace SGBU.UI.Navegacion;

// Abre y cierra las ventanas, para que los ViewModels no tengan que conocer las vistas.
public class ServicioVentanas : IServicioVentanas
{
    private readonly ServicioAutenticacion autenticacion;
    private readonly ServicioCatalogo catalogo;
    private readonly ServicioUsuarios usuarios;
    private readonly ServicioPrestamos prestamos;
    private readonly ServicioMultas multas;
    private readonly ServicioReportes reportes;
    private Window? ventanaActual;

    public ServicioVentanas(ServicioAutenticacion autenticacion, ServicioCatalogo catalogo, ServicioUsuarios usuarios, ServicioPrestamos prestamos, ServicioMultas multas, ServicioReportes reportes)
    {
        this.autenticacion = autenticacion;
        this.catalogo = catalogo;
        this.usuarios = usuarios;
        this.prestamos = prestamos;
        this.multas = multas;
        this.reportes = reportes;
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
        principal.DataContext = new PrincipalViewModel(sesion, this, catalogo, usuarios, prestamos, multas, reportes);
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
