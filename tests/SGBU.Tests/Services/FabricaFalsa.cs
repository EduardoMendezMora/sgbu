using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

// "Base de datos" en memoria para probar los servicios sin SQL Server.
internal class FabricaFalsa : IFabricaUnidadDeTrabajo
{
    public List<Usuario> Usuarios { get; } = new List<Usuario>();

    public List<Rol> Roles { get; } = new List<Rol>();

    public List<Material> Materiales { get; } = new List<Material>();

    public List<RegistroAuditoria> Auditoria { get; } = new List<RegistroAuditoria>();

    public int Confirmaciones { get; set; }

    public Task<IUnidadDeTrabajo> CrearAsync()
    {
        return Task.FromResult<IUnidadDeTrabajo>(new UnidadFalsa(this));
    }
}
