using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

public class RepositorioUsuarios : IRepositorioUsuarios
{
    // Se une con Roles porque el usuario necesita los datos de su rol.
    private const string SelectUsuarios =
        "SELECT u.*, r.Nombre AS RolNombre, r.LimitePrestamos, r.DiasPrestamo " +
        "FROM Usuarios u INNER JOIN Roles r ON r.Id = u.RolId";

    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;

    public RepositorioUsuarios(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        using SqlCommand cmd = new SqlCommand(SelectUsuarios + " WHERE u.Id = @Id", conexion, transaccion);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        return await LeerUnoAsync(cmd);
    }

    public async Task<Usuario?> ObtenerPorCarneAsync(string carne)
    {
        using SqlCommand cmd = new SqlCommand(SelectUsuarios + " WHERE u.Carne = @Carne", conexion, transaccion);
        cmd.Parameters.Add("@Carne", SqlDbType.NVarChar, 20).Value = carne.Trim();
        return await LeerUnoAsync(cmd);
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        using SqlCommand cmd = new SqlCommand(SelectUsuarios + " WHERE u.Email = @Email", conexion, transaccion);
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 120).Value = email.Trim();
        return await LeerUnoAsync(cmd);
    }

    public async Task<Pagina<Usuario>> BuscarAsync(string? texto, int numeroPagina, int tamanoPagina)
    {
        string filtro = " WHERE (@Texto IS NULL OR u.Carne LIKE @Texto OR u.Nombre LIKE @Texto " +
                        "OR u.Apellidos LIKE @Texto OR u.Email LIKE @Texto)";
        object valorTexto = string.IsNullOrWhiteSpace(texto) ? DBNull.Value : "%" + texto.Trim() + "%";

        using SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Usuarios u" + filtro, conexion, transaccion);
        cmdTotal.Parameters.Add("@Texto", SqlDbType.NVarChar, 130).Value = valorTexto;
        int total = (int)(await cmdTotal.ExecuteScalarAsync())!;

        string sql = SelectUsuarios + filtro + " ORDER BY u.Apellidos, u.Nombre OFFSET @Saltar ROWS FETCH NEXT @Tamano ROWS ONLY";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@Texto", SqlDbType.NVarChar, 130).Value = valorTexto;
        cmd.Parameters.Add("@Saltar", SqlDbType.Int).Value = (numeroPagina - 1) * tamanoPagina;
        cmd.Parameters.Add("@Tamano", SqlDbType.Int).Value = tamanoPagina;

        List<Usuario> lista = new List<Usuario>();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(LeerUsuario(reader));
        }

        return new Pagina<Usuario>(lista, numeroPagina, tamanoPagina, total);
    }

    public async Task<int> AgregarAsync(Usuario usuario)
    {
        string sql = "INSERT INTO Usuarios (Carne, Dni, Nombre, Apellidos, Email, Telefono, RolId, PasswordHash, Activo) " +
                     "OUTPUT INSERTED.Id " +
                     "VALUES (@Carne, @Dni, @Nombre, @Apellidos, @Email, @Telefono, @RolId, @PasswordHash, @Activo)";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        AgregarParametros(cmd, usuario);

        try
        {
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new DatoInvalidoException(MensajeDuplicado(ex));
        }
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        string sql = "UPDATE Usuarios SET Carne = @Carne, Dni = @Dni, Nombre = @Nombre, Apellidos = @Apellidos, " +
                     "Email = @Email, Telefono = @Telefono, RolId = @RolId, PasswordHash = @PasswordHash, Activo = @Activo " +
                     "WHERE Id = @Id";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        AgregarParametros(cmd, usuario);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = usuario.Id;

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new DatoInvalidoException(MensajeDuplicado(ex));
        }
    }

    // El mensaje de SQL Server trae el nombre de la restricción que se rompió.
    private static string MensajeDuplicado(SqlException ex)
    {
        if (ex.Message.Contains("UQ_Usuarios_Email"))
        {
            return "Ya existe un usuario con ese email.";
        }

        if (ex.Message.Contains("UQ_Usuarios_Dni"))
        {
            return "Ya existe un usuario con ese DNI.";
        }

        return "Ya existe un usuario con ese carné.";
    }

    private static void AgregarParametros(SqlCommand cmd, Usuario u)
    {
        cmd.Parameters.Add("@Carne", SqlDbType.NVarChar, 20).Value = u.Carne;
        cmd.Parameters.Add("@Dni", SqlDbType.NVarChar, 20).Value = u.Dni;
        cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 60).Value = u.Nombre;
        cmd.Parameters.Add("@Apellidos", SqlDbType.NVarChar, 80).Value = u.Apellidos;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 120).Value = u.Email;
        cmd.Parameters.Add("@Telefono", SqlDbType.NVarChar, 20).Value = (object?)u.Telefono ?? DBNull.Value;
        cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = u.Rol.Id;
        cmd.Parameters.Add("@PasswordHash", SqlDbType.Char, 60).Value = u.PasswordHash;
        cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = u.Activo;
    }

    private static async Task<Usuario?> LeerUnoAsync(SqlCommand cmd)
    {
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return LeerUsuario(reader);
        }

        return null;
    }

    private static Usuario LeerUsuario(SqlDataReader reader)
    {
        Rol rol = new Rol((int)reader["RolId"], (string)reader["RolNombre"], (int)reader["LimitePrestamos"], (int)reader["DiasPrestamo"]);

        Usuario usuario = new Usuario(
            (string)reader["Carne"],
            (string)reader["Dni"],
            (string)reader["Nombre"],
            (string)reader["Apellidos"],
            (string)reader["Email"],
            reader["Telefono"] as string,
            rol,
            (string)reader["PasswordHash"])
        {
            Id = (int)reader["Id"],
            FechaRegistro = (DateTime)reader["FechaRegistro"],
        };

        if (!(bool)reader["Activo"])
        {
            usuario.Desactivar();
        }

        return usuario;
    }
}
