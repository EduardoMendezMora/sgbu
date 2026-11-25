using System.Data;

namespace SGBU.Domain.Repositorios;

// Consultas de los reportes. Devuelven un DataTable porque así la misma tabla se puede
// mostrar en pantalla, exportar a PDF y exportar a Excel sin escribir una clase por reporte.
public interface IRepositorioReportes
{
    Task<DataTable> TopMasPrestadosAsync();

    Task<DataTable> UsuariosConMultasAsync();

    Task<DataTable> StockCriticoAsync(int limite);

    Task<DataTable> VencidosHoyAsync();

    // Materiales, usuarios (sin contraseña) y préstamos, para el respaldo en XML.
    Task<DataSet> DatosParaRespaldoAsync();
}
