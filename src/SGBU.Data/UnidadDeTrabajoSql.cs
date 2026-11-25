using Microsoft.Data.SqlClient;
using SGBU.Data.Repositorios;
using SGBU.Domain.Repositorios;

namespace SGBU.Data;

// Unit of Work: una sola conexión y una sola transacción para todos los repositorios.
// Si no se llama a ConfirmarAsync, al cerrar se deshace todo (rollback).
public class UnidadDeTrabajoSql : IUnidadDeTrabajo
{
    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;
    private bool terminada;

    private UnidadDeTrabajoSql(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;

        Materiales = new RepositorioMateriales(conexion, transaccion);
        Usuarios = new RepositorioUsuarios(conexion, transaccion);
        Prestamos = new RepositorioPrestamos(conexion, transaccion);
        Multas = new RepositorioMultas(conexion, transaccion);
        Reservas = new RepositorioReservas(conexion, transaccion);
        Auditoria = new RepositorioAuditoria(conexion, transaccion);
        Reportes = new RepositorioReportes(conexion, transaccion);
    }

    public IRepositorioMateriales Materiales { get; }

    public IRepositorioUsuarios Usuarios { get; }

    public IRepositorioPrestamos Prestamos { get; }

    public IRepositorioMultas Multas { get; }

    public IRepositorioReservas Reservas { get; }

    public IRepositorioAuditoria Auditoria { get; }

    public IRepositorioReportes Reportes { get; }

    // Abre la conexión y empieza la transacción.
    public static async Task<UnidadDeTrabajoSql> AbrirAsync(string cadenaConexion)
    {
        SqlConnection conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync();
        SqlTransaction transaccion = conexion.BeginTransaction();
        return new UnidadDeTrabajoSql(conexion, transaccion);
    }

    public async Task ConfirmarAsync()
    {
        if (terminada)
        {
            throw new InvalidOperationException("La transacción ya terminó.");
        }

        await transaccion.CommitAsync();
        terminada = true;
    }

    public async Task RevertirAsync()
    {
        if (terminada)
        {
            throw new InvalidOperationException("La transacción ya terminó.");
        }

        await transaccion.RollbackAsync();
        terminada = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!terminada)
        {
            await RevertirAsync();
        }

        await transaccion.DisposeAsync();
        await conexion.DisposeAsync();
    }
}
