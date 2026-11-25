namespace SGBU.Domain.Repositorios;

/// <summary>
/// Unit of Work: agrupa varios repositorios en UNA transacción.
/// Ejemplo, un préstamo: baja el stock, crea el préstamo y escribe la auditoría. Si falla
/// cualquiera de los tres, no se guarda ninguno (RevertirAsync); si todo sale bien, ConfirmarAsync.
/// </summary>
public interface IUnidadDeTrabajo : IAsyncDisposable
{
    IRepositorioMateriales Materiales { get; }

    IRepositorioUsuarios Usuarios { get; }

    IRepositorioPrestamos Prestamos { get; }

    IRepositorioMultas Multas { get; }

    IRepositorioReservas Reservas { get; }

    IRepositorioAuditoria Auditoria { get; }

    IRepositorioReportes Reportes { get; }

    Task ConfirmarAsync();

    Task RevertirAsync();
}
