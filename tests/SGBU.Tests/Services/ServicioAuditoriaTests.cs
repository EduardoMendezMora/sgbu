using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Services.Auditoria;
using SGBU.Services.Seguridad;
using Xunit;

namespace SGBU.Tests.Services;

public class ServicioAuditoriaTests
{
    private static readonly Sesion Admin = new(1, "Ana Rojas", "Administrador", DateTime.Now);
    private static readonly Sesion Profesor = new(2, "Ricardo Salas", "Profesor", DateTime.Now);

    [Fact]
    public async Task Buscar_FiltraPorAccionYFechas()
    {
        var (servicio, _) = Crear();

        var pagina = await servicio.BuscarAsync(Admin, "login", new DateTime(2026, 11, 20), new DateTime(2026, 11, 21), 1);

        Assert.Equal(2, pagina.Total);
    }

    [Fact]
    public async Task Buscar_DesdeDespuesDeHasta_DaDatoInvalido()
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<DatoInvalidoException>(() =>
            servicio.BuscarAsync(Admin, null, new DateTime(2026, 11, 21), new DateTime(2026, 11, 1), 1));
    }

    [Fact]
    public async Task Buscar_SoloAdministrador()
    {
        var (servicio, _) = Crear();

        await Assert.ThrowsAsync<OperacionNoPermitidaException>(() =>
            servicio.BuscarAsync(Profesor, null, DateTime.Today, DateTime.Today, 1));
    }

    [Fact]
    public void Sesion_TodosPuedenCambiarSusPreferencias_PeroSoloAdminConfigura()
    {
        Assert.True(new Sesion(4, "X", "Estudiante", DateTime.Now).Puede(Permiso.Preferencias));
        Assert.True(Profesor.Puede(Permiso.Preferencias));
        Assert.False(Profesor.Puede(Permiso.Configurar));
    }

    private static (ServicioAuditoria Servicio, FabricaFalsa Fabrica) Crear()
    {
        var fabrica = new FabricaFalsa();
        fabrica.Auditoria.Add(new RegistroAuditoria(1, "LOGIN", "Usuarios", 1, null, new DateTime(2026, 11, 20, 8, 0, 0)));
        fabrica.Auditoria.Add(new RegistroAuditoria(4, "LOGIN_FALLIDO", "Usuarios", 4, null, new DateTime(2026, 11, 21, 23, 0, 0)));
        fabrica.Auditoria.Add(new RegistroAuditoria(1, "PRESTAMO", "Prestamos", 9, null, new DateTime(2026, 11, 20, 9, 0, 0)));
        fabrica.Auditoria.Add(new RegistroAuditoria(1, "LOGIN", "Usuarios", 1, null, new DateTime(2026, 11, 5, 9, 0, 0)));
        return (new ServicioAuditoria(fabrica), fabrica);
    }
}
