using System.Data;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SGBU.Services.Reportes;

// Exporta cualquier DataTable a PDF (QuestPDF) o a Excel (EPPlus).
// El nombre de la tabla (TableName) es el título del reporte.
public static class Exportador
{
    public static void ExportarPdf(DataTable tabla, string ruta)
    {
        // Licencia gratuita de QuestPDF para proyectos pequeños y académicos.
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.Letter);
                pagina.Margin(30);
                pagina.DefaultTextStyle(texto => texto.FontSize(10));

                pagina.Header().Column(columna =>
                {
                    columna.Item().Text("SGBU - Biblioteca Universitaria").FontSize(9).FontColor(Colors.Grey.Darken1);
                    columna.Item().Text(tabla.TableName).FontSize(16).Bold();
                    columna.Item().Text("Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9);
                });

                pagina.Content().PaddingTop(10).Table(t =>
                {
                    t.ColumnsDefinition(columnas =>
                    {
                        foreach (DataColumn unused in tabla.Columns)
                        {
                            columnas.RelativeColumn();
                        }
                    });

                    // Encabezados
                    foreach (DataColumn columna in tabla.Columns)
                    {
                        t.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(columna.ColumnName).Bold();
                    }

                    // Filas
                    foreach (DataRow fila in tabla.Rows)
                    {
                        foreach (DataColumn columna in tabla.Columns)
                        {
                            t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(4).Text(Formatear(fila[columna]));
                        }
                    }
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        }).GeneratePdf(ruta);
    }

    public static void ExportarExcel(DataTable tabla, string ruta)
    {
        // EPPlus pide indicar que el uso es no comercial.
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using ExcelPackage paquete = new ExcelPackage();
        ExcelWorksheet hoja = paquete.Workbook.Worksheets.Add("Reporte");

        hoja.Cells[1, 1].Value = tabla.TableName;
        hoja.Cells[1, 1].Style.Font.Bold = true;
        hoja.Cells[1, 1].Style.Font.Size = 14;

        // La tabla empieza en la fila 3, con encabezados.
        hoja.Cells[3, 1].LoadFromDataTable(tabla, true);
        hoja.Cells[3, 1, 3, tabla.Columns.Count].Style.Font.Bold = true;

        // Las fechas se muestran como fecha y no como número.
        for (int i = 0; i < tabla.Columns.Count; i++)
        {
            if (tabla.Columns[i].DataType == typeof(DateTime))
            {
                hoja.Column(i + 1).Style.Numberformat.Format = "dd/mm/yyyy";
            }
        }

        hoja.Cells[hoja.Dimension.Address].AutoFitColumns();
        paquete.SaveAs(new FileInfo(ruta));
    }

    private static string Formatear(object valor)
    {
        if (valor is DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy");
        }

        if (valor is decimal numero)
        {
            return numero.ToString("N0");
        }

        return valor.ToString() ?? string.Empty;
    }
}
