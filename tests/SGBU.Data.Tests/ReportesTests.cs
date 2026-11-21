using Xunit;

namespace SGBU.Data.Tests;

public class ReportesTests
{
    [FactConBase]
    public async Task TopMasPrestados_OrdenaPorVecesPrestado()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var tabla = await uow.Reportes.TopMasPrestadosAsync();

        Assert.InRange(tabla.Rows.Count, 1, 10);
        Assert.Equal("Clean Code", tabla.Rows[0]["Título"]);
        Assert.Equal(3, tabla.Rows[0]["Veces prestado"]);
    }

    [FactConBase]
    public async Task UsuariosConMultas_TraeAMariaConSuTotal()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var tabla = await uow.Reportes.UsuariosConMultasAsync();

        Assert.Equal(1, tabla.Rows.Count);
        Assert.Equal("EST-1002", tabla.Rows[0]["Carné"]);
        Assert.Equal(400m, tabla.Rows[0]["Total pendiente"]);
    }

    [FactConBase]
    public async Task StockCritico_TraeLosQueTienenTresOMenos()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var tabla = await uow.Reportes.StockCriticoAsync(3);

        Assert.True(tabla.Rows.Count > 0);
        foreach (System.Data.DataRow fila in tabla.Rows)
        {
            Assert.True((int)fila["Disponibles"] <= 3);
        }
    }

    [FactConBase]
    public async Task VencidosHoy_TraeLosDosDelSeed()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var tabla = await uow.Reportes.VencidosHoyAsync();

        Assert.Equal(2, tabla.Rows.Count);
    }

    [FactConBase]
    public async Task Respaldo_TraeTresTablasYNoIncluyeContrasenas()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();

        var datos = await uow.Reportes.DatosParaRespaldoAsync();

        Assert.Equal(new[] { "Material", "Usuario", "Prestamo" }, datos.Tables.Cast<System.Data.DataTable>().Select(t => t.TableName));
        Assert.False(datos.Tables["Usuario"]!.Columns.Contains("PasswordHash"));
        Assert.Equal(14, datos.Tables["Material"]!.Rows.Count);
    }
}
