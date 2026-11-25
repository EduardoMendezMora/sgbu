using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Tests.Services;

internal class UnidadFalsa : IUnidadDeTrabajo
{
    private readonly FabricaFalsa fabrica;
    private readonly List<RegistroAuditoria> pendientes = new List<RegistroAuditoria>();

    public UnidadFalsa(FabricaFalsa fabrica)
    {
        this.fabrica = fabrica;
        Usuarios = new UsuariosFalsos(fabrica);
        Materiales = new MaterialesFalsos(fabrica.Materiales);
        Prestamos = new PrestamosFalsos(fabrica);
        Multas = new MultasFalsas(fabrica);
        Reservas = new ReservasFalsas(fabrica);
        Auditoria = new AuditoriaFalsa(pendientes, fabrica.Auditoria);
    }

    public IRepositorioMateriales Materiales { get; }

    public IRepositorioUsuarios Usuarios { get; }

    public IRepositorioPrestamos Prestamos { get; }

    public IRepositorioMultas Multas { get; }

    public IRepositorioReservas Reservas { get; }

    public IRepositorioAuditoria Auditoria { get; }

    public IRepositorioReportes Reportes => new ReportesFalsos();

    public Task ConfirmarAsync()
    {
        fabrica.Auditoria.AddRange(pendientes);
        fabrica.Confirmaciones++;
        return Task.CompletedTask;
    }

    public Task RevertirAsync()
    {
        pendientes.Clear();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
