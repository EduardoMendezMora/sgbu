using SGBU.Domain.Entidades;

namespace SGBU.Services.Usuarios;

// Los datos que llegan del formulario de la pantalla Usuarios.
public class FormularioUsuario
{
    public int Id { get; set; }

    public string Carne { get; set; } = string.Empty;

    public string Dni { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public int RolId { get; set; }

    // Obligatoria para un usuario nuevo. Al editar, si se deja vacía se conserva la anterior.
    public string Contrasena { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public static FormularioUsuario Desde(Usuario usuario)
    {
        FormularioUsuario datos = new FormularioUsuario();
        datos.Id = usuario.Id;
        datos.Carne = usuario.Carne;
        datos.Dni = usuario.Dni;
        datos.Nombre = usuario.Nombre;
        datos.Apellidos = usuario.Apellidos;
        datos.Email = usuario.Email;
        datos.Telefono = usuario.Telefono;
        datos.RolId = usuario.Rol.Id;
        datos.Activo = usuario.Activo;
        return datos;
    }
}
