using SGBU.Domain.Entidades;
using Xunit;

namespace SGBU.Data.Tests;

public class AuditoriaTests
{
    [FactConBase]
    public async Task Buscar_EncuentraLoRegistradoConElNombreDelUsuario()
    {
        await using var uow = await BaseDeDatos.AbrirAsync();
        var admin = (await uow.Usuarios.ObtenerPorCarneAsync("ADM-001"))!;
        var accion = "PRUEBA_" + Guid.NewGuid().ToString("N")[..6];
        await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(admin.Id, accion, "Materiales", 1, "detalle", DateTime.Now));
        await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(null, accion, null, null, null, DateTime.Now));

        var pagina = await uow.Auditoria.BuscarAsync(accion, DateTime.Today, DateTime.Today, 1, 10);

        Assert.Equal(2, pagina.Total);
        Assert.Contains(pagina.Elementos, r => r.Usuario.Contains("Ana Rojas"));
        Assert.Contains(pagina.Elementos, r => r.Usuario == "(sistema)");
    }
}
