using System.Data;
using OfficeOpenXml;
using SGBU.Domain.Excepciones;
using SGBU.Services.Reportes;
using SGBU.Services.Seguridad;
using Xunit;

namespace SGBU.Tests.Services;

public class ServicioReportesTests
{
    private static readonly Sesion Admin = new(1, "Ana Rojas", "Administrador", DateTime.Now);
    private static readonly Sesion Profesor = new(2, "Ricardo Salas", "Profesor", DateTime.Now);
    private static readonly Sesion Estudiante = new(4, "Carlos Jiménez", "Estudiante", DateTime.Now);

    [Fact]
    public async Task Generar_TopMasPrestados_DevuelveLaTablaConTitulo()
    {
        var servicio = new ServicioReportes(new FabricaFalsa());

        var tabla = await servicio.GenerarAsync(Profesor, TipoReporte.TopMasPrestados);

        Assert.Equal("Top 10 materiales más prestados", tabla.TableName);
        Assert.Equal(2, tabla.Rows.Count);
    }

    [Fact]
    public async Task Generar_StockCritico_UsaElLimiteDeTres()
    {
        var servicio = new ServicioReportes(new FabricaFalsa());

        var tabla = await servicio.GenerarAsync(Admin, TipoReporte.StockCritico);

        Assert.Equal(3, tabla.Rows[0][0]);
    }

    [Fact]
    public async Task Generar_EstudianteNoPuedeVerReportes()
    {
        var servicio = new ServicioReportes(new FabricaFalsa());

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.GenerarAsync(Estudiante, TipoReporte.VencidosHoy));
    }

    [Fact]
    public void ExportarPdf_CreaUnArchivoPdf()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "sgbu_prueba_" + Guid.NewGuid() + ".pdf");

        Exportador.ExportarPdf(TablaDePrueba(), ruta);

        var inicio = File.ReadAllBytes(ruta).Take(4).ToArray();
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(inicio));
        File.Delete(ruta);
    }

    [Fact]
    public void ExportarExcel_CreaUnaHojaConEncabezadosYDatos()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "sgbu_prueba_" + Guid.NewGuid() + ".xlsx");

        Exportador.ExportarExcel(TablaDePrueba(), ruta);

        using (var paquete = new ExcelPackage(new FileInfo(ruta)))
        {
            var hoja = paquete.Workbook.Worksheets[0];
            Assert.Equal("Top de prueba", hoja.Cells[1, 1].Text);
            Assert.Equal("Título", hoja.Cells[3, 1].Text);
            Assert.Equal("Clean Code", hoja.Cells[4, 1].Text);
            Assert.Equal(4, Convert.ToInt32(hoja.Cells[4, 2].Value));
        }

        File.Delete(ruta);
    }

    [Fact]
    public async Task Respaldo_SoloAdministrador_YCreaUnXmlConLosDatos()
    {
        var servicio = new ServicioReportes(new FabricaFalsa());
        var ruta = Path.Combine(Path.GetTempPath(), "sgbu_respaldo_" + Guid.NewGuid() + ".xml");

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() => servicio.ExportarRespaldoXmlAsync(Profesor, ruta));
        await servicio.ExportarRespaldoXmlAsync(Admin, ruta);

        var leido = new DataSet();
        leido.ReadXml(ruta);
        Assert.Equal("Clean Code", leido.Tables["Material"]!.Rows[0]["Titulo"]);
        File.Delete(ruta);
    }

    private static DataTable TablaDePrueba()
    {
        var tabla = new DataTable("Top de prueba");
        tabla.Columns.Add("Título");
        tabla.Columns.Add("Veces prestado", typeof(int));
        tabla.Rows.Add("Clean Code", 4);
        tabla.Rows.Add("Effective Java", 2);
        return tabla;
    }
}
