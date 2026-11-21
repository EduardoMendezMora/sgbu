using System.Windows;
using Microsoft.Extensions.Configuration;
using SGBU.Data;
using SGBU.Services.Catalogo;
using SGBU.Services.Prestamos;
using SGBU.Services.Seguridad;
using SGBU.Services.Usuarios;
using SGBU.UI.Navegacion;

namespace SGBU.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Se lee la cadena de conexión de appsettings.json (o de appsettings.Local.json si existe).
        IConfiguration configuracion = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();

        string? cadena = configuracion.GetConnectionString("Sgbu");
        if (string.IsNullOrWhiteSpace(cadena))
        {
            MessageBox.Show("Falta la cadena de conexión en appsettings.json.", "SGBU");
            Shutdown();
            return;
        }

        // Aquí se crean los objetos y se le pasa a cada uno lo que necesita.
        FabricaUnidadDeTrabajoSql fabrica = new FabricaUnidadDeTrabajoSql(cadena);
        HasherBCrypt hasher = new HasherBCrypt();
        ServicioAutenticacion autenticacion = new ServicioAutenticacion(fabrica, hasher);
        ServicioCatalogo catalogo = new ServicioCatalogo(fabrica);
        ServicioUsuarios usuarios = new ServicioUsuarios(fabrica, hasher);
        ServicioPrestamos prestamos = new ServicioPrestamos(fabrica);

        ServicioVentanas ventanas = new ServicioVentanas(autenticacion, catalogo, usuarios, prestamos);
        ventanas.MostrarLogin();
    }
}
