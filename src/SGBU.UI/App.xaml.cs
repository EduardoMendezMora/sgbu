using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGBU.Data;
using SGBU.Domain.Repositorios;
using SGBU.Services.Comun;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;
using SGBU.UI.ViewModels;
using SGBU.UI.Views;

namespace SGBU.UI;

/// <summary>
/// Punto de entrada. Aquí, y solo aquí, se arma la inyección de dependencias ("composition
/// root"): es el único lugar que conoce las clases concretas de todas las capas. El resto
/// del programa pide interfaces por el constructor.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? proveedor;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Orden de prioridad: variable de entorno SGBU_CONN > appsettings.Local.json > appsettings.json.
        var configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();
        var cadena = Environment.GetEnvironmentVariable("SGBU_CONN")
            ?? configuracion.GetConnectionString("Sgbu")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Sgbu' en appsettings.json.");

        var servicios = new ServiceCollection();
        servicios.AddSingleton<IFabricaUnidadDeTrabajo>(new FabricaUnidadDeTrabajoSql(cadena));
        servicios.AddSingleton<IReloj, RelojSistema>();
        servicios.AddSingleton<IHasherContrasenas, HasherBCrypt>();
        servicios.AddTransient<ServicioAutenticacion>();
        servicios.AddSingleton<IServicioVentanas, ServicioVentanas>();
        servicios.AddTransient<LoginViewModel>();
        servicios.AddTransient<LoginWindow>();
        proveedor = servicios.BuildServiceProvider();

        proveedor.GetRequiredService<IServicioVentanas>().MostrarLogin();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        proveedor?.Dispose();
        base.OnExit(e);
    }
}
