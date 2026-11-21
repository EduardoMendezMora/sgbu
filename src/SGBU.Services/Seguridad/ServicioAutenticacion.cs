using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;

namespace SGBU.Services.Seguridad;

// Caso de uso: iniciar sesión.
public class ServicioAutenticacion
{
    private readonly IFabricaUnidadDeTrabajo fabrica;
    private readonly IHasherContrasenas hasher;

    public ServicioAutenticacion(IFabricaUnidadDeTrabajo fabrica, IHasherContrasenas hasher)
    {
        this.fabrica = fabrica;
        this.hasher = hasher;
    }

    // "usuario" puede ser el email o el carné.
    public async Task<Sesion> IniciarSesionAsync(string usuario, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            throw new DatoInvalidoException("Escriba su usuario y su contraseña.");
        }

        usuario = usuario.Trim();
        await using IUnidadDeTrabajo uow = await fabrica.CrearAsync();

        Usuario? encontrado;
        if (usuario.Contains('@'))
        {
            encontrado = await uow.Usuarios.ObtenerPorEmailAsync(usuario);
        }
        else
        {
            encontrado = await uow.Usuarios.ObtenerPorCarneAsync(usuario);
        }

        // Si no existe o la clave está mal se da el mismo mensaje,
        // para no revelar qué usuarios están registrados.
        if (encontrado == null || !hasher.Verificar(contrasena, encontrado.PasswordHash))
        {
            int? usuarioId = encontrado?.Id;
            await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(usuarioId, "LOGIN_FALLIDO", "Usuarios", usuarioId, "Intento con " + usuario, DateTime.Now));
            await uow.ConfirmarAsync();
            throw new CredencialesInvalidasException();
        }

        if (!encontrado.Activo)
        {
            throw new OperacionNoPermitidaException("Su usuario está inactivo. Consulte con la biblioteca.");
        }

        await uow.Auditoria.RegistrarAsync(new RegistroAuditoria(encontrado.Id, "LOGIN", "Usuarios", encontrado.Id, null, DateTime.Now));
        await uow.ConfirmarAsync();

        return new Sesion(encontrado.Id, encontrado.NombreCompleto, encontrado.Rol.Nombre, DateTime.Now);
    }
}
