using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Excepciones;
using SGBU.Services.Comun;
using SGBU.Services.Seguridad;
using Xunit;

namespace SGBU.Data.Tests;

/// <summary>
/// El login completo: servicio real + repositorios reales + SQL Server, con los usuarios del seed.
/// Este caso sí confirma (escribe en Auditoria), así que al final borra lo que escribió.
/// </summary>
public class LoginDePuntaAPuntaTests
{
    [FactConBase]
    public async Task Admin_EntraConLaClaveDelSeed_YQuedaAuditado()
    {
        var servicio = new ServicioAutenticacion(
            new FabricaUnidadDeTrabajoSql(BaseDeDatos.Cadena!), new HasherBCrypt(), new RelojSistema());
        var desde = DateTime.Now.AddSeconds(-1);

        try
        {
            var sesion = await servicio.IniciarSesionAsync("admin@sgbu.ac.cr", "Sgbu2026!");
            await Assert.ThrowsAsync<CredencialesInvalidasException>(() => servicio.IniciarSesionAsync("ADM-001", "mala"));

            Assert.Equal("Administrador", sesion.Rol);
            Assert.True(sesion.Puede(Permiso.VerAuditoria));
            Assert.Equal(2, await ContarAuditoriaAsync(sesion.UsuarioId, desde, borrar: false));
        }
        finally
        {
            await ContarAuditoriaAsync(null, desde, borrar: true);
        }
    }

    private static async Task<int> ContarAuditoriaAsync(int? usuarioId, DateTime desde, bool borrar)
    {
        var sql = borrar
            ? "DELETE dbo.Auditoria WHERE Accion IN (N'LOGIN', N'LOGIN_FALLIDO') AND Fecha >= @desde AND Entidad = N'Usuarios'; SELECT @@ROWCOUNT;"
            : "SELECT COUNT(*) FROM dbo.Auditoria WHERE UsuarioId = @u AND Accion IN (N'LOGIN', N'LOGIN_FALLIDO') AND Fecha >= @desde;";
        await using var conexion = new SqlConnection(BaseDeDatos.Cadena);
        await conexion.OpenAsync();
        await using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@u", SqlDbType.Int).Value = (object?)usuarioId ?? DBNull.Value;
        comando.Parameters.Add("@desde", SqlDbType.DateTime2).Value = desde;
        return (int)(await comando.ExecuteScalarAsync())!;
    }
}
