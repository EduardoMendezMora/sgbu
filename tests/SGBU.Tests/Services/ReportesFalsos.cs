using System.Data;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class ReportesFalsos : IRepositorioReportes
{
    public Task<DataTable> TopMasPrestadosAsync()
    {
        DataTable tabla = new DataTable();
        tabla.Columns.Add("Título");
        tabla.Columns.Add("Veces prestado", typeof(int));
        tabla.Rows.Add("Clean Code", 4);
        tabla.Rows.Add("Effective Java", 2);
        return Task.FromResult(tabla);
    }

    public Task<DataTable> UsuariosConMultasAsync() => Task.FromResult(new DataTable());

    public Task<DataTable> StockCriticoAsync(int limite)
    {
        DataTable tabla = new DataTable();
        tabla.Columns.Add("Límite usado", typeof(int));
        tabla.Rows.Add(limite);
        return Task.FromResult(tabla);
    }

    public Task<DataTable> VencidosHoyAsync() => Task.FromResult(new DataTable());

    public Task<DataSet> DatosParaRespaldoAsync()
    {
        DataSet datos = new DataSet("SGBU");
        DataTable materiales = datos.Tables.Add("Material");
        materiales.Columns.Add("Isbn");
        materiales.Columns.Add("Titulo");
        materiales.Rows.Add("9780132350884", "Clean Code");
        return Task.FromResult(datos);
    }
}
