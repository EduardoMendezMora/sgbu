using SGBU.Services.Auditoria;
using SGBU.Services.Catalogo;
using SGBU.Services.Multas;
using SGBU.Services.Prestamos;
using SGBU.Services.Reportes;
using SGBU.Services.Seguridad;
using SGBU.Services.Usuarios;

namespace SGBU.UI.Navegacion;

// Junta todos los servicios en un solo objeto para no pasar siete parámetros a cada ventana.
public class Servicios
{
    public Servicios(
        ServicioAutenticacion autenticacion,
        ServicioCatalogo catalogo,
        ServicioUsuarios usuarios,
        ServicioPrestamos prestamos,
        ServicioMultas multas,
        ServicioReportes reportes,
        ServicioAuditoria auditoria)
    {
        Autenticacion = autenticacion;
        Catalogo = catalogo;
        Usuarios = usuarios;
        Prestamos = prestamos;
        Multas = multas;
        Reportes = reportes;
        Auditoria = auditoria;
    }

    public ServicioAutenticacion Autenticacion { get; }

    public ServicioCatalogo Catalogo { get; }

    public ServicioUsuarios Usuarios { get; }

    public ServicioPrestamos Prestamos { get; }

    public ServicioMultas Multas { get; }

    public ServicioReportes Reportes { get; }

    public ServicioAuditoria Auditoria { get; }
}
