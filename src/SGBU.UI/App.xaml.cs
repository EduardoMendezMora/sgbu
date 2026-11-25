using System.Windows;
using Microsoft.Extensions.Configuration;
using SGBU.Data;
using SGBU.Services.Auditoria;
using SGBU.Services.Catalogo;
using SGBU.Services.Configuracion;
using SGBU.Services.Multas;
using SGBU.Services.Prestamos;
using SGBU.Services.Reportes;
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

        // Tema e idioma guardados en esta computadora.
        Apariencia.Aplicar(Preferencias.Cargar(Preferencias.RutaPorDefecto()));

        // Aquí se crean los objetos y se le pasa a cada uno lo que necesita.
        FabricaUnidadDeTrabajoSql fabrica = new FabricaUnidadDeTrabajoSql(cadena);
        HasherBCrypt hasher = new HasherBCrypt();
        Servicios servicios = new Servicios(
            new ServicioAutenticacion(fabrica, hasher),
            new ServicioCatalogo(fabrica),
            new ServicioUsuarios(fabrica, hasher),
            new ServicioPrestamos(fabrica),
            new ServicioMultas(fabrica),
            new ServicioReportes(fabrica),
            new ServicioAuditoria(fabrica));

        ServicioVentanas ventanas = new ServicioVentanas(servicios);
        ventanas.MostrarLogin();
    }
}
