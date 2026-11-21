using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Seguridad;

namespace SGBU.Services.Usuarios;

// Casos de uso de la pantalla Usuarios (solo para el administrador).
public class ServicioUsuarios
{
    public const int TamanoPagina = 10;
    private const int LargoMinimoContrasena = 8;

    private readonly IFabricaUnidadDeTrabajo fabrica;
    private readonly IHasherContrasenas hasher;

    public ServicioUsuarios(IFabricaUnidadDeTrabajo fabrica, IHasherContrasenas hasher)
    {
        this.fabrica = fabrica;
        this.hasher = hasher;
    }

    public async Task<Pagina<Usuario>> BuscarAsync(Sesion sesion, string? texto, int numeroPagina)
    {
        RevisarPermiso(sesion);
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Usuarios.BuscarAsync(texto, numeroPagina, TamanoPagina);
    }

    public async Task<IReadOnlyList<Rol>> ListarRolesAsync()
    {
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();
        return await uow.Usuarios.ListarRolesAsync();
    }

    public async Task GuardarAsync(Sesion sesion, FormularioUsuario datos)
    {
        RevisarPermiso(sesion);
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        IReadOnlyList<Rol> roles = await uow.Usuarios.ListarRolesAsync();
        Rol? rol = roles.FirstOrDefault(r => r.Id == datos.RolId);
        if (rol == null)
        {
            throw new DatoInvalidoException("Seleccione un rol.");
        }

        string? telefono = string.IsNullOrWhiteSpace(datos.Telefono) ? null : datos.Telefono;

        if (datos.Id == 0)
        {
            string hash = GenerarHash(datos.Contrasena);
            Usuario nuevo = new Usuario(datos.Carne, datos.Dni, datos.Nombre, datos.Apellidos, datos.Email, telefono, rol, hash);
            if (!datos.Activo)
            {
                nuevo.Desactivar();
            }

            int id = await uow.Usuarios.AgregarAsync(nuevo);
            await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(sesion.UsuarioId, "USUARIO_CREADO", "Usuarios", id, nuevo.Carne, DateTime.Now));
        }
        else
        {
            Usuario? actual = await uow.Usuarios.ObtenerPorIdAsync(datos.Id);
            if (actual == null)
            {
                throw new DatoInvalidoException("El usuario ya no existe.");
            }

            // Si no escribió contraseña nueva, se deja la que tenía.
            string hash = actual.PasswordHash;
            if (!string.IsNullOrWhiteSpace(datos.Contrasena))
            {
                hash = GenerarHash(datos.Contrasena);
            }

            Usuario editado = new Usuario(datos.Carne, datos.Dni, datos.Nombre, datos.Apellidos, datos.Email, telefono, rol, hash)
            {
                Id = datos.Id,
                FechaRegistro = actual.FechaRegistro,
            };
            if (!datos.Activo)
            {
                editado.Desactivar();
            }

            await uow.Usuarios.ActualizarAsync(editado);
            await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(sesion.UsuarioId, "USUARIO_EDITADO", "Usuarios", datos.Id, editado.Carne, DateTime.Now));
        }

        await uow.ConfirmarAsync();
    }

    private static void RevisarPermiso(Sesion sesion)
    {
        if (!sesion.Puede(Permiso.GestionarUsuarios))
        {
            throw new OperacionNoPermitidaException("Solo el administrador puede gestionar usuarios.");
        }
    }

    private string GenerarHash(string contrasena)
    {
        if (contrasena == null || contrasena.Length < LargoMinimoContrasena)
        {
            throw new DatoInvalidoException("La contraseña debe tener al menos " + LargoMinimoContrasena + " caracteres.");
        }

        return hasher.Generar(contrasena);
    }
}
