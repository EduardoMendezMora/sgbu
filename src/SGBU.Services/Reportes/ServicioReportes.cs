using System.Data;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Reglas;
using SGBU.Domain.Repositorios;
using SGBU.Services.Seguridad;

namespace SGBU.Services.Reportes;

// Genera los cuatro reportes que pide el enunciado y el respaldo en XML.
public class ServicioReportes
{
    private readonly IFabricaUnidadDeTrabajo fabrica;

    public ServicioReportes(IFabricaUnidadDeTrabajo fabrica)
    {
        this.fabrica = fabrica;
    }

    public static string Titulo(TipoReporte tipo)
    {
        switch (tipo)
        {
            case TipoReporte.TopMasPrestados:
                return "Top 10 materiales más prestados";
            case TipoReporte.UsuariosConMultas:
                return "Usuarios con multas pendientes";
            case TipoReporte.StockCritico:
                return "Materiales con stock crítico (3 o menos)";
            default:
                return "Préstamos que vencen hoy";
        }
    }

    public async Task<DataTable> GenerarAsync(Sesion sesion, TipoReporte tipo)
    {
        if (!sesion.Puede(Permiso.VerReportes))
        {
            throw new OperacionNoPermitidaException("No tiene permiso para ver reportes.");
        }

        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        DataTable tabla;
        switch (tipo)
        {
            case TipoReporte.TopMasPrestados:
                tabla = await uow.Reportes.TopMasPrestadosAsync();
                break;
            case TipoReporte.UsuariosConMultas:
                tabla = await uow.Reportes.UsuariosConMultasAsync();
                break;
            case TipoReporte.StockCritico:
                tabla = await uow.Reportes.StockCriticoAsync(ReglasBiblioteca.StockCritico);
                break;
            default:
                tabla = await uow.Reportes.VencidosHoyAsync();
                break;
        }

        // El nombre de la tabla se usa como título en el PDF y en el Excel.
        tabla.TableName = Titulo(tipo);
        return tabla;
    }

    // Guarda materiales, usuarios y préstamos en un archivo XML (con su esquema).
    public async Task ExportarRespaldoXmlAsync(Sesion sesion, string ruta)
    {
        if (!sesion.Puede(Permiso.Configurar))
        {
            throw new OperacionNoPermitidaException("Solo el administrador puede generar el respaldo.");
        }

        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        DataSet datos = await uow.Reportes.DatosParaRespaldoAsync();
        datos.WriteXml(ruta, XmlWriteMode.WriteSchema);

        await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(sesion.UsuarioId, "RESPALDO_XML", null, null, Path.GetFileName(ruta), DateTime.Now));
        await uow.ConfirmarAsync();
    }
}
