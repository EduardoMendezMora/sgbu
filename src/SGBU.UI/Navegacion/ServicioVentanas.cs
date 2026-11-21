using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SGBU.Services.Seguridad;
using SGBU.UI.ViewModels;
using SGBU.UI.Views;

namespace SGBU.UI.Navegacion;

public sealed class ServicioVentanas : IServicioVentanas
{
    private readonly IServiceProvider proveedor;
    private Window? actual;

    public ServicioVentanas(IServiceProvider proveedor) => this.proveedor = proveedor;

    public void MostrarLogin() => Cambiar(proveedor.GetRequiredService<LoginWindow>());

    public void MostrarPrincipal(Sesion sesion) =>
        Cambiar(new PrincipalWindow { DataContext = new PrincipalViewModel(sesion, this) });

    public void CerrarSesion() => MostrarLogin();

    private void Cambiar(Window nueva)
    {
        var anterior = actual;
        actual = nueva;
        nueva.Closed += AlCerrar;
        nueva.Show();

        if (anterior is not null)
        {
            anterior.Closed -= AlCerrar;
            anterior.Close();
        }
    }

    // Si el usuario cierra la ventana con la X, se termina la aplicación.
    private void AlCerrar(object? sender, EventArgs e) => Application.Current.Shutdown();
}
