using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Data.Infraestructura;
using SGBU.Domain.Entidades;
using SGBU.Domain.Reglas;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

/// <summary>
/// Materiales con ADO.NET directo. La tabla es una sola (columna Tipo) y aquí se decide
/// qué clase de C# crear: Libro, Revista o Tesis. Esa traducción es el "mapeo" que un ORM
/// haría solo; el enunciado pide hacerlo a mano para entender qué pasa por debajo.
/// </summary>
internal sealed class RepositorioMateriales : IRepositorioMateriales
{
    private const string Columnas = """
        Id, Tipo, Isbn, Titulo, Autor, Editorial, AnioPublicacion, Categoria, StockTotal, StockDisponible,
        Activo, Edicion, NumeroVolumen, Periodicidad, Universidad, GradoAcademico, Tutor
        """;

    private const string Filtro = """
        WHERE (@patron IS NULL OR Titulo LIKE @patron ESCAPE '\' OR Autor LIKE @patron ESCAPE '\' OR Isbn LIKE @patron ESCAPE '\')
          AND (@tipo IS NULL OR Tipo = @tipo)
        """;

    private readonly ContextoSql contexto;

    public RepositorioMateriales(ContextoSql contexto) => this.contexto = contexto;

    public Task<Material?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var comando = contexto.Comando($"SELECT {Columnas} FROM dbo.Materiales WHERE Id = @id;")
            .Con("@id", SqlDbType.Int, id);
        return LeerUnoAsync(comando, ct);
    }

    public Task<Material?> ObtenerPorIsbnAsync(string isbn, CancellationToken ct = default)
    {
        if (!Isbn.EsValido(isbn))
        {
            return Task.FromResult<Material?>(null);
        }

        var comando = contexto.Comando($"SELECT {Columnas} FROM dbo.Materiales WHERE Isbn = @isbn;")
            .Con("@isbn", SqlDbType.Char, Isbn.Normalizar(isbn), 13);
        return LeerUnoAsync(comando, ct);
    }

    /// <summary>
    /// Paginación del lado del servidor: un solo viaje a la base trae el total (primer
    /// resultado) y solo las filas de la página pedida con OFFSET/FETCH (segundo resultado).
    /// </summary>
    public async Task<Pagina<Material>> BuscarAsync(string? texto, TipoMaterial? tipo, int numeroPagina, int tamanoPagina, CancellationToken ct = default)
    {
        var saltar = ExtensionesSql.Saltar(numeroPagina, tamanoPagina);
        await using var comando = contexto.Comando($"""
            SELECT COUNT(*) FROM dbo.Materiales {Filtro};
            SELECT {Columnas} FROM dbo.Materiales {Filtro}
            ORDER BY Titulo, Id OFFSET @saltar ROWS FETCH NEXT @tamano ROWS ONLY;
            """)
            .Con("@patron", SqlDbType.NVarChar, ExtensionesSql.PatronContiene(texto), 210)
            .Con("@tipo", SqlDbType.NVarChar, tipo?.ToString(), 10)
            .Con("@saltar", SqlDbType.Int, saltar)
            .Con("@tamano", SqlDbType.Int, tamanoPagina);

        await using var lector = await comando.ExecuteReaderAsync(ct);
        await lector.ReadAsync(ct);
        var total = lector.GetInt32(0);

        await lector.NextResultAsync(ct);
        var elementos = new List<Material>();
        while (await lector.ReadAsync(ct))
        {
            elementos.Add(Mapear(lector));
        }

        return new Pagina<Material>(elementos, numeroPagina, tamanoPagina, total);
    }

    public Task<int> AgregarAsync(Material material, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            INSERT dbo.Materiales (Tipo, Isbn, Titulo, Autor, Editorial, AnioPublicacion, Categoria, StockTotal,
                StockDisponible, Activo, Edicion, NumeroVolumen, Periodicidad, Universidad, GradoAcademico, Tutor)
            OUTPUT INSERTED.Id
            VALUES (@tipo, @isbn, @titulo, @autor, @editorial, @anio, @categoria, @total,
                @disponible, @activo, @edicion, @volumen, @periodicidad, @universidad, @grado, @tutor);
            """);
        AgregarParametros(comando, material);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                return (int)(await comando.ExecuteScalarAsync(ct))!;
            }
        });
    }

    public Task ActualizarAsync(Material material, CancellationToken ct = default)
    {
        var comando = contexto.Comando("""
            UPDATE dbo.Materiales SET Tipo = @tipo, Isbn = @isbn, Titulo = @titulo, Autor = @autor,
                Editorial = @editorial, AnioPublicacion = @anio, Categoria = @categoria, StockTotal = @total,
                StockDisponible = @disponible, Activo = @activo, Edicion = @edicion, NumeroVolumen = @volumen,
                Periodicidad = @periodicidad, Universidad = @universidad, GradoAcademico = @grado, Tutor = @tutor
            WHERE Id = @id;
            """).Con("@id", SqlDbType.Int, material.Id);
        AgregarParametros(comando, material);

        return TraductorErroresSql.EjecutarAsync(async () =>
        {
            await using (comando)
            {
                await comando.ExecuteNonQueryAsync(ct);
            }
        });
    }

    private static void AgregarParametros(SqlCommand comando, Material m)
    {
        var libro = m as Libro;
        var revista = m as Revista;
        var tesis = m as Tesis;

        comando
            .Con("@tipo", SqlDbType.NVarChar, m.Tipo.ToString(), 10)
            .Con("@isbn", SqlDbType.Char, m.Isbn, 13)
            .Con("@titulo", SqlDbType.NVarChar, m.Titulo, 200)
            .Con("@autor", SqlDbType.NVarChar, m.Autor, 150)
            .Con("@editorial", SqlDbType.NVarChar, m.Editorial, 100)
            .Con("@anio", SqlDbType.SmallInt, m.AnioPublicacion)
            .Con("@categoria", SqlDbType.NVarChar, m.Categoria, 60)
            .Con("@total", SqlDbType.Int, m.StockTotal)
            .Con("@disponible", SqlDbType.Int, m.StockDisponible)
            .Con("@activo", SqlDbType.Bit, m.Activo)
            .Con("@edicion", SqlDbType.NVarChar, libro?.Edicion, 30)
            .Con("@volumen", SqlDbType.NVarChar, revista?.NumeroVolumen, 30)
            .Con("@periodicidad", SqlDbType.NVarChar, revista?.Periodicidad, 30)
            .Con("@universidad", SqlDbType.NVarChar, tesis?.Universidad, 120)
            .Con("@grado", SqlDbType.NVarChar, tesis?.GradoAcademico, 40)
            .Con("@tutor", SqlDbType.NVarChar, tesis?.Tutor, 120);
    }

    private static async Task<Material?> LeerUnoAsync(SqlCommand comando, CancellationToken ct)
    {
        await using (comando)
        {
            await using var lector = await comando.ExecuteReaderAsync(ct);
            return await lector.ReadAsync(ct) ? Mapear(lector) : null;
        }
    }

    /// <summary>La columna Tipo decide qué subclase se construye.</summary>
    private static Material Mapear(SqlDataReader r)
    {
        var id = r.Entero("Id");
        var isbn = r.Texto("Isbn");
        var titulo = r.Texto("Titulo");
        var autor = r.Texto("Autor");
        var total = r.Entero("StockTotal");
        var disponible = r.Entero("StockDisponible");
        var editorial = r.TextoONulo("Editorial");
        var anio = r.CortoONulo("AnioPublicacion");
        var categoria = r.TextoONulo("Categoria");
        var activo = r.Booleano("Activo");

        return Enum.Parse<TipoMaterial>(r.Texto("Tipo")) switch
        {
            TipoMaterial.Libro => new Libro(id, isbn, titulo, autor, total, disponible, r.TextoONulo("Edicion"))
            {
                Editorial = editorial, AnioPublicacion = anio, Categoria = categoria, Activo = activo,
            },
            TipoMaterial.Revista => new Revista(id, isbn, titulo, autor, total, disponible, r.TextoONulo("NumeroVolumen"), r.TextoONulo("Periodicidad"))
            {
                Editorial = editorial, AnioPublicacion = anio, Categoria = categoria, Activo = activo,
            },
            TipoMaterial.Tesis => new Tesis(id, isbn, titulo, autor, total, disponible, r.Texto("Universidad"), r.TextoONulo("GradoAcademico"), r.TextoONulo("Tutor"))
            {
                Editorial = editorial, AnioPublicacion = anio, Categoria = categoria, Activo = activo,
            },
            _ => throw new InvalidOperationException("Tipo de material desconocido en la base."),
        };
    }
}
