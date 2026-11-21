using Microsoft.Data.SqlClient;
using SGBU.Data.Infraestructura;
using SGBU.Data.Repositorios;
using SGBU.Domain.Repositorios;

namespace SGBU.Data;

/// <summary>
/// Unit of Work con ADO.NET: UNA conexión y UNA transacción compartidas por todos los
/// repositorios. Se usa así:
/// <code>
/// await using var uow = await fabrica.CrearAsync();
/// ... trabajar con uow.Materiales, uow.Prestamos, uow.Auditoria ...
/// await uow.ConfirmarAsync();   // si no se llega aquí, todo se revierte solo
/// </code>
/// Si ocurre una excepción antes de ConfirmarAsync, el DisposeAsync revierte. Así nunca
/// queda un préstamo guardado sin que haya bajado el stock, o al revés.
/// </summary>
public sealed class UnidadDeTrabajoSql : IUnidadDeTrabajo
{
    private readonly SqlConnection conexion;
    private readonly ContextoSql contexto;

    private UnidadDeTrabajoSql(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        contexto = new ContextoSql(conexion, transaccion);
        Materiales = new RepositorioMateriales(contexto);
        Usuarios = new RepositorioUsuarios(contexto);
        Prestamos = new RepositorioPrestamos(contexto);
        Multas = new RepositorioMultas(contexto);
        Reservas = new RepositorioReservas(contexto);
        Auditoria = new RepositorioAuditoria(contexto);
    }

    public IRepositorioMateriales Materiales { get; }

    public IRepositorioUsuarios Usuarios { get; }

    public IRepositorioPrestamos Prestamos { get; }

    public IRepositorioMultas Multas { get; }

    public IRepositorioReservas Reservas { get; }

    public IRepositorioAuditoria Auditoria { get; }

    public static async Task<UnidadDeTrabajoSql> AbrirAsync(string cadenaConexion, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);
        var conexion = new SqlConnection(cadenaConexion);
        try
        {
            await conexion.OpenAsync(ct);
            var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(ct);
            return new UnidadDeTrabajoSql(conexion, transaccion);
        }
        catch
        {
            await conexion.DisposeAsync();
            throw;
        }
    }

    public async Task ConfirmarAsync(CancellationToken ct = default)
    {
        var transaccion = TerminarTransaccion();
        await using (transaccion)
        {
            await transaccion.CommitAsync(ct);
        }
    }

    public async Task RevertirAsync(CancellationToken ct = default)
    {
        var transaccion = TerminarTransaccion();
        await using (transaccion)
        {
            await transaccion.RollbackAsync(ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (contexto.Transaccion is not null)
        {
            await RevertirAsync();
        }

        await conexion.DisposeAsync();
    }

    private SqlTransaction TerminarTransaccion()
    {
        var transaccion = contexto.Transaccion
            ?? throw new InvalidOperationException("La transacción ya se confirmó o se revirtió.");
        contexto.Transaccion = null;
        return transaccion;
    }
}
