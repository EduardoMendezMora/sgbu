using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Seguridad;

namespace SGBU.Services.Prestamos;

// Casos de uso de préstamos. Primero se revisan las reglas con las clases de Domain
// (así el mensaje de error es claro) y después se llama al stored procedure, que hace
// los cambios en la base dentro de una transacción y vuelve a revisar stock y límite.
public class ServicioPrestamos
{
    private readonly IFabricaUnidadDeTrabajo fabrica;

    public ServicioPrestamos(IFabricaUnidadDeTrabajo fabrica)
    {
        this.fabrica = fabrica;
    }

    public async Task<Prestamo> RegistrarAsync(Sesion sesion, string carne, string isbn)
    {
        RevisarPermiso(sesion);

        if (string.IsNullOrWhiteSpace(carne) || string.IsNullOrWhiteSpace(isbn))
        {
            throw new DatoInvalidoException("Escriba el carné del usuario y el ISBN del material.");
        }

        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        Usuario? usuario = await uow.Usuarios.ObtenerPorCarneAsync(carne);
        if (usuario == null)
        {
            throw new DatoInvalidoException("No existe un usuario con el carné " + carne + ".");
        }

        Material? material = await uow.Materiales.ObtenerPorIsbnAsync(isbn);
        if (material == null)
        {
            throw new DatoInvalidoException("No existe un material con el ISBN " + isbn + ".");
        }

        int activos = await uow.Prestamos.ContarActivosDeUsuarioAsync(usuario.Id);
        bool tieneMultas = await uow.Multas.TieneMultasPendientesAsync(usuario.Id);
        usuario.ValidarPuedePrestar(activos, tieneMultas);

        if (material.StockDisponible == 0)
        {
            throw new StockInsuficienteException(material.Titulo);
        }

        int id = await uow.Prestamos.RegistrarAsync(usuario.Id, material.Id, sesion.UsuarioId);
        Prestamo? prestamo = await uow.Prestamos.ObtenerPorIdAsync(id);
        await uow.ConfirmarAsync();
        return prestamo!;
    }

    public async Task<ResultadoDevolucion> DevolverAsync(Sesion sesion, int prestamoId)
    {
        RevisarPermiso(sesion);
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        ResultadoDevolucion resultado = await uow.Prestamos.DevolverAsync(prestamoId, sesion.UsuarioId);
        await uow.ConfirmarAsync();
        return resultado;
    }

    // Puede renovar el bibliotecario o el mismo usuario dueño del préstamo.
    public async Task RenovarAsync(Sesion sesion, int prestamoId)
    {
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        Prestamo? prestamo = await uow.Prestamos.ObtenerPorIdAsync(prestamoId);
        if (prestamo == null)
        {
            throw new DatoInvalidoException("El préstamo no existe.");
        }

        if (prestamo.UsuarioId != sesion.UsuarioId && !sesion.Puede(Permiso.RegistrarPrestamos))
        {
            throw new OperacionNoPermitidaException("Solo puede renovar sus propios préstamos.");
        }

        Usuario? usuario = await uow.Usuarios.ObtenerPorIdAsync(prestamo.UsuarioId);
        bool reservado = await uow.Reservas.ExistePendienteAsync(prestamo.MaterialId);

        // Domain revisa las reglas (una sola vez, no vencido, no reservado).
        prestamo.Renovar(usuario!.Rol, reservado, DateTime.Now);

        await uow.Prestamos.RenovarAsync(prestamoId, sesion.UsuarioId);
        await uow.ConfirmarAsync();
    }

    public async Task<IReadOnlyList<PrestamoDetalle>> ListarActivosAsync(Sesion sesion, string? texto)
    {
        RevisarPermiso(sesion);
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Prestamos.ListarActivosDetalleAsync(texto, null);
    }

    public async Task<IReadOnlyList<PrestamoDetalle>> ListarMisPrestamosAsync(Sesion sesion)
    {
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Prestamos.ListarActivosDetalleAsync(null, sesion.UsuarioId);
    }

    private static void RevisarPermiso(Sesion sesion)
    {
        if (!sesion.Puede(Permiso.RegistrarPrestamos))
        {
            throw new OperacionNoPermitidaException("No tiene permiso para registrar préstamos.");
        }
    }
}
