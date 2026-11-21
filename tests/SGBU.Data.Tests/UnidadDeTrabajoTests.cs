using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using Xunit;

namespace SGBU.Data.Tests;

public class UnidadDeTrabajoTests
{
    [FactConBase]
    public async Task Revertir_DeshaceTodoLoHechoEnLaTransaccion()
    {
        int id;
        await using (var uow = await BaseDeDatos.AbrirAsync())
        {
            id = await uow.Materiales.AgregarAsync(new Libro("9780000000026", "Se revierte", "Nadie", 1));
            await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(null, "PRUEBA", "Materiales", id, null, DateTime.Now));
            await uow.RevertirAsync();
        }

        await using var otra = await BaseDeDatos.AbrirAsync();
        Assert.Null(await otra.Materiales.ObtenerPorIsbnAsync("9780000000026"));
    }

    [FactConBase]
    public async Task Confirmar_DosVeces_NoSePermite()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        await uow.RevertirAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.ConfirmarAsync());
    }

    [FactConBase]
    public async Task Confirmar_GuardaDeVerdad()
    {
        var accion = "PRUEBA_" + Guid.NewGuid().ToString("N")[..8];
        await using (var uow = await BaseDeDatos.AbrirAsync())
        {
            await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(null, accion, null, null, "prueba de integración", DateTime.Now));
            await uow.ConfirmarAsync();
        }

        // Se revisa con una conexión aparte y se limpia lo que se guardó.
        await using var conexion = new SqlConnection(BaseDeDatos.Cadena);
        await conexion.OpenAsync();
        await using var comando = new SqlCommand(
            "SELECT COUNT(*) FROM dbo.Auditoria WHERE Accion = @a; DELETE dbo.Auditoria WHERE Accion = @a;", conexion);
        comando.Parameters.Add("@a", SqlDbType.NVarChar, 50).Value = accion;
        Assert.Equal(1, (int)(await comando.ExecuteScalarAsync())!);
    }
}
