using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Data.Infraestructura;
using SGBU.Domain.Entidades;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

/// <summary>Usuarios con su rol (JOIN a Roles: el rol trae los límites de préstamo).</summary>
internal sealed class RepositorioUsuarios : IRepositorioUsuarios
{
    private const string Consulta = """
        SELECT u.Id, u.Carne, u.Dni, u.Nombre, u.Apellidos, u.Email, u.Telefono, u.PasswordHash, u.Activo,
               u.FechaRegistro, r.Id AS RolId, r.Nombre AS RolNombre, r.LimitePrestamos, r.DiasPrestamo
        FROM dbo.Usuarios u
        JOIN dbo.Roles r ON r.Id = u.RolId
        """;

    private const string Filtro = """
        WHERE (@patron IS NULL OR u.Carne LIKE @patron ESCAPE '\' OR u.Nombre LIKE @patron ESCAPE '\'
            OR u.Apellidos LIKE @patron ESCAPE '\' OR u.Email LIKE @patron ESCAPE '\')
        """;

    private readonly ContextoSql contexto;

    public RepositorioUsuarios(ContextoSql contexto) => this.contexto = contexto;

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        LeerUnoAsync(contexto.Comando($"{Consulta} WHERE u.Id = @v;").Con("@v", SqlDbType.Int, id), ct);

    public Task<Usuario?> ObtenerPorCarneAsync(string carne, CancellationToken ct = default) =>
        LeerUnoAsync(contexto.Comando($"{Consulta} WHERE u.Carne = @v;").Con("@v", SqlDbType.NVarChar, carne.Trim(), 20), ct);

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default) =>
        LeerUnoAsync(contexto.Comando($"{Consulta} WHERE u.Email = @v;").Con("@v", SqlDbType.NVarChar, email.Trim(), 120), ct);

    public async Task<Pagina<Usuario>> BuscarAsync(string? texto, int numeroPagina, int tamanoPagina, CancellationToken ct = default)
    {
        var saltar = ExtensionesSql.Saltar(numeroPagina, tamanoPagina);
        await using var comando = contexto.Comando($"""
            SELECT COUNT(*) FROM dbo.Usuarios u {Filtro};
            {Consulta} {Filtro}
            ORDER BY u.Apellidos, u.Nombre, u.Id OFFSET @saltar ROWS FETCH NEXT @tamano ROWS ONLY;
            """)
            .Con("@patron", SqlDbType.NVarChar, ExtensionesSql.PatronContiene(texto), 130)
            .Con("@saltar", SqlDbType.Int, saltar)
            .Con("@tamano", SqlDbType.Int, tamanoPagina);

        await using var lector = await comando.ExecuteReaderAsync(ct);
        await lector.ReadAsync(ct);
        var total = lector.GetInt32(0);

        await lector.NextResultAsync(ct);
        var elementos = new List<Usuario>();
        while (await lector.ReadAsync(ct))
        {
            elementos.Add(Mapear(lector));
        }

        return new Pagina<Usuario>(elementos, numeroPagina, tamanoPagina, total);
    }

    public Task<int> AgregarAsync(Usuario usuario, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            INSERT dbo.Usuarios (Carne, Dni, Nombre, Apellidos, Email, Telefono, RolId, PasswordHash, Activo)
            OUTPUT INSERTED.Id
            VALUES (@carne, @dni, @nombre, @apellidos, @email, @telefono, @rol, @hash, @activo);
            """);
        AgregarParametros(comando, usuario);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                return (int)(await comando.ExecuteScalarAsync(ct))!;
            }
        });
    }

    public Task ActualizarAsync(Usuario usuario, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            UPDATE dbo.Usuarios SET Carne = @carne, Dni = @dni, Nombre = @nombre, Apellidos = @apellidos,
                Email = @email, Telefono = @telefono, RolId = @rol, PasswordHash = @hash, Activo = @activo
            WHERE Id = @id;
            """).Con("@id", SqlDbType.Int, usuario.Id);
        AgregarParametros(comando, usuario);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                await comando.ExecuteNonQueryAsync(ct);
            }
        });
    }

    private static void AgregarParametros(SqlCommand comando, Usuario u) =>
        comando
            .Con("@carne", SqlDbType.NVarChar, u.Carne, 20)
            .Con("@dni", SqlDbType.NVarChar, u.Dni, 20)
            .Con("@nombre", SqlDbType.NVarChar, u.Nombre, 60)
            .Con("@apellidos", SqlDbType.NVarChar, u.Apellidos, 80)
            .Con("@email", SqlDbType.NVarChar, u.Email, 120)
            .Con("@telefono", SqlDbType.NVarChar, u.Telefono, 20)
            .Con("@rol", SqlDbType.Int, u.Rol.Id)
            .Con("@hash", SqlDbType.Char, u.PasswordHash, 60)
            .Con("@activo", SqlDbType.Bit, u.Activo);

    private static async Task<Usuario?> LeerUnoAsync(SqlCommand comando, CancellationToken ct)
    {
        await using (comando)
        {
            await using var lector = await comando.ExecuteReaderAsync(ct);
            return await lector.ReadAsync(ct) ? Mapear(lector) : null;
        }
    }

    private static Usuario Mapear(SqlDataReader r)
    {
        var rol = new Rol(r.Entero("RolId"), r.Texto("RolNombre"), r.Entero("LimitePrestamos"), r.Entero("DiasPrestamo"));
        var usuario = new Usuario(
            r.Texto("Carne"), r.Texto("Dni"), r.Texto("Nombre"), r.Texto("Apellidos"),
            r.Texto("Email"), r.TextoONulo("Telefono"), rol, r.Texto("PasswordHash"))
        {
            Id = r.Entero("Id"),
            FechaRegistro = r.Fecha("FechaRegistro"),
        };

        if (!r.Booleano("Activo"))
        {
            usuario.Desactivar();
        }

        return usuario;
    }
}
