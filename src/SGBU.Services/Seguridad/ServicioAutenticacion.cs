using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Comun;

namespace SGBU.Services.Seguridad;

/// <summary>
/// Caso de uso "Iniciar sesión". Recibe sus dependencias por el constructor (inyección de
/// dependencias): no sabe si detrás hay SQL Server o una lista en memoria, y por eso se
/// puede probar sin base de datos.
/// </summary>
public sealed class ServicioAutenticacion
{
    // Hash de una clave cualquiera. Se usa cuando el usuario no existe, para que la
    // respuesta tarde lo mismo que con una clave incorrecta: si "no existe" respondiera más
    // rápido, midiendo el tiempo se sabría qué correos están registrados.
    private const string HashDeRelleno = "$2b$11$vjXiYk5DCKFhMyyikFj60.hwFToLc0LKwceoGPBPZZyC8F7QIm4Vy";

    private readonly IFabricaUnidadDeTrabajo fabrica;
    private readonly IHasherContrasenas hasher;
    private readonly IReloj reloj;

    public ServicioAutenticacion(IFabricaUnidadDeTrabajo fabrica, IHasherContrasenas hasher, IReloj reloj)
    {
        this.fabrica = fabrica;
        this.hasher = hasher;
        this.reloj = reloj;
    }

    /// <param name="usuario">Email o carné.</param>
    public async Task<Sesion> IniciarSesionAsync(string usuario, string contrasena, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            throw new DatoInvalidoException("Escriba su usuario y su contraseña.");
        }

        var identificador = usuario.Trim();
        await using var uow = await fabrica.CrearAsync(ct);

        var encontrado = identificador.Contains('@', StringComparison.Ordinal)
            ? await uow.Usuarios.ObtenerPorEmailAsync(identificador, ct)
            : await uow.Usuarios.ObtenerPorCarneAsync(identificador, ct);

        var claveCorrecta = hasher.Verificar(contrasena, encontrado?.PasswordHash ?? HashDeRelleno);

        if (encontrado is null || !claveCorrecta)
        {
            // El intento fallido se guarda aunque el login no siga: sirve para detectar ataques.
            await RegistrarAsync(uow, encontrado?.Id, "LOGIN_FALLIDO", $"Intento con \"{identificador}\"", ct);
            await uow.ConfirmarAsync(ct);
            throw new CredencialesInvalidasException();
        }

        if (!encontrado.Activo)
        {
            throw new OperacionNoPermitidaException("Su usuario está inactivo. Consulte con la biblioteca.");
        }

        await RegistrarAsync(uow, encontrado.Id, "LOGIN", null, ct);
        await uow.ConfirmarAsync(ct);

        return new Sesion(encontrado.Id, encontrado.NombreCompleto, encontrado.Rol.Nombre, reloj.Ahora);
    }

    private Task RegistrarAsync(IUnidadDeTrabajo uow, int? usuarioId, string accion, string? detalle, CancellationToken ct) =>
        uow.Auditoria.RegistrarAsync(new RegistroAuditoria(usuarioId, accion, "Usuarios", usuarioId, detalle, reloj.Ahora), ct);
}
