using System.Data;
using Microsoft.Data.SqlClient;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Reglas;
using SGBU.Domain.Repositorios;

namespace SGBU.Data.Repositorios;

public class RepositorioMateriales : IRepositorioMateriales
{
    private readonly SqlConnection conexion;
    private readonly SqlTransaction transaccion;

    public RepositorioMateriales(SqlConnection conexion, SqlTransaction transaccion)
    {
        this.conexion = conexion;
        this.transaccion = transaccion;
    }

    public async Task<Material?> ObtenerPorIdAsync(int id)
    {
        string sql = "SELECT * FROM Materiales WHERE Id = @Id";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return LeerMaterial(reader);
        }

        return null;
    }

    public async Task<Material?> ObtenerPorIsbnAsync(string isbn)
    {
        if (!Isbn.EsValido(isbn))
        {
            return null;
        }

        string sql = "SELECT * FROM Materiales WHERE Isbn = @Isbn";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        cmd.Parameters.Add("@Isbn", SqlDbType.Char, 13).Value = Isbn.Normalizar(isbn);

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return LeerMaterial(reader);
        }

        return null;
    }

    // Paginación en el servidor: primero se cuenta el total y luego se traen solo
    // las filas de la página con OFFSET / FETCH.
    public async Task<Pagina<Material>> BuscarAsync(string? texto, TipoMaterial? tipo, int numeroPagina, int tamanoPagina)
    {
        string filtro = " WHERE (@Texto IS NULL OR Titulo LIKE @Texto OR Autor LIKE @Texto OR Isbn LIKE @Texto)" +
                        " AND (@Tipo IS NULL OR Tipo = @Tipo)";

        using SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Materiales" + filtro, conexion, transaccion);
        AgregarFiltros(cmdTotal, texto, tipo);
        int total = (int)(await cmdTotal.ExecuteScalarAsync())!;

        string sql = "SELECT * FROM Materiales" + filtro +
                     " ORDER BY Titulo OFFSET @Saltar ROWS FETCH NEXT @Tamano ROWS ONLY";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        AgregarFiltros(cmd, texto, tipo);
        cmd.Parameters.Add("@Saltar", SqlDbType.Int).Value = (numeroPagina - 1) * tamanoPagina;
        cmd.Parameters.Add("@Tamano", SqlDbType.Int).Value = tamanoPagina;

        List<Material> lista = new List<Material>();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(LeerMaterial(reader));
        }

        return new Pagina<Material>(lista, numeroPagina, tamanoPagina, total);
    }

    public async Task<int> AgregarAsync(Material material)
    {
        string sql = "INSERT INTO Materiales (Tipo, Isbn, Titulo, Autor, Editorial, AnioPublicacion, Categoria, " +
                     "StockTotal, StockDisponible, Activo, Edicion, NumeroVolumen, Periodicidad, Universidad, GradoAcademico, Tutor) " +
                     "OUTPUT INSERTED.Id " +
                     "VALUES (@Tipo, @Isbn, @Titulo, @Autor, @Editorial, @Anio, @Categoria, " +
                     "@StockTotal, @StockDisponible, @Activo, @Edicion, @NumeroVolumen, @Periodicidad, @Universidad, @Grado, @Tutor)";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        AgregarParametros(cmd, material);

        try
        {
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new DatoInvalidoException("Ya existe un material con ese ISBN.");
        }
    }

    public async Task ActualizarAsync(Material material)
    {
        string sql = "UPDATE Materiales SET Tipo = @Tipo, Isbn = @Isbn, Titulo = @Titulo, Autor = @Autor, " +
                     "Editorial = @Editorial, AnioPublicacion = @Anio, Categoria = @Categoria, StockTotal = @StockTotal, " +
                     "StockDisponible = @StockDisponible, Activo = @Activo, Edicion = @Edicion, NumeroVolumen = @NumeroVolumen, " +
                     "Periodicidad = @Periodicidad, Universidad = @Universidad, GradoAcademico = @Grado, Tutor = @Tutor " +
                     "WHERE Id = @Id";
        using SqlCommand cmd = new SqlCommand(sql, conexion, transaccion);
        AgregarParametros(cmd, material);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = material.Id;

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new DatoInvalidoException("Ya existe un material con ese ISBN.");
        }
    }

    private static void AgregarFiltros(SqlCommand cmd, string? texto, TipoMaterial? tipo)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            cmd.Parameters.Add("@Texto", SqlDbType.NVarChar, 210).Value = DBNull.Value;
        }
        else
        {
            cmd.Parameters.Add("@Texto", SqlDbType.NVarChar, 210).Value = "%" + texto.Trim() + "%";
        }

        if (tipo == null)
        {
            cmd.Parameters.Add("@Tipo", SqlDbType.NVarChar, 10).Value = DBNull.Value;
        }
        else
        {
            cmd.Parameters.Add("@Tipo", SqlDbType.NVarChar, 10).Value = tipo.ToString();
        }
    }

    private static void AgregarParametros(SqlCommand cmd, Material m)
    {
        cmd.Parameters.Add("@Tipo", SqlDbType.NVarChar, 10).Value = m.Tipo.ToString();
        cmd.Parameters.Add("@Isbn", SqlDbType.Char, 13).Value = m.Isbn;
        cmd.Parameters.Add("@Titulo", SqlDbType.NVarChar, 200).Value = m.Titulo;
        cmd.Parameters.Add("@Autor", SqlDbType.NVarChar, 150).Value = m.Autor;
        cmd.Parameters.Add("@Editorial", SqlDbType.NVarChar, 100).Value = (object?)m.Editorial ?? DBNull.Value;
        cmd.Parameters.Add("@Anio", SqlDbType.SmallInt).Value = (object?)m.AnioPublicacion ?? DBNull.Value;
        cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 60).Value = (object?)m.Categoria ?? DBNull.Value;
        cmd.Parameters.Add("@StockTotal", SqlDbType.Int).Value = m.StockTotal;
        cmd.Parameters.Add("@StockDisponible", SqlDbType.Int).Value = m.StockDisponible;
        cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = m.Activo;

        // Campos propios de cada tipo: los que no aplican se guardan en NULL.
        object edicion = DBNull.Value;
        object numeroVolumen = DBNull.Value;
        object periodicidad = DBNull.Value;
        object universidad = DBNull.Value;
        object grado = DBNull.Value;
        object tutor = DBNull.Value;

        if (m is Libro libro)
        {
            edicion = (object?)libro.Edicion ?? DBNull.Value;
        }
        else if (m is Revista revista)
        {
            numeroVolumen = (object?)revista.NumeroVolumen ?? DBNull.Value;
            periodicidad = (object?)revista.Periodicidad ?? DBNull.Value;
        }
        else if (m is Tesis tesis)
        {
            universidad = tesis.Universidad;
            grado = (object?)tesis.GradoAcademico ?? DBNull.Value;
            tutor = (object?)tesis.Tutor ?? DBNull.Value;
        }

        cmd.Parameters.Add("@Edicion", SqlDbType.NVarChar, 30).Value = edicion;
        cmd.Parameters.Add("@NumeroVolumen", SqlDbType.NVarChar, 30).Value = numeroVolumen;
        cmd.Parameters.Add("@Periodicidad", SqlDbType.NVarChar, 30).Value = periodicidad;
        cmd.Parameters.Add("@Universidad", SqlDbType.NVarChar, 120).Value = universidad;
        cmd.Parameters.Add("@Grado", SqlDbType.NVarChar, 40).Value = grado;
        cmd.Parameters.Add("@Tutor", SqlDbType.NVarChar, 120).Value = tutor;
    }

    // Según la columna Tipo se crea un Libro, una Revista o una Tesis (herencia).
    private static Material LeerMaterial(SqlDataReader reader)
    {
        int id = (int)reader["Id"];
        string tipo = (string)reader["Tipo"];
        string isbn = (string)reader["Isbn"];
        string titulo = (string)reader["Titulo"];
        string autor = (string)reader["Autor"];
        int stockTotal = (int)reader["StockTotal"];
        int stockDisponible = (int)reader["StockDisponible"];

        Material material;
        if (tipo == "Libro")
        {
            material = new Libro(id, isbn, titulo, autor, stockTotal, stockDisponible, reader["Edicion"] as string)
            {
                Editorial = reader["Editorial"] as string,
                AnioPublicacion = reader["AnioPublicacion"] as short?,
                Categoria = reader["Categoria"] as string,
                Activo = (bool)reader["Activo"],
            };
        }
        else if (tipo == "Revista")
        {
            material = new Revista(id, isbn, titulo, autor, stockTotal, stockDisponible, reader["NumeroVolumen"] as string, reader["Periodicidad"] as string)
            {
                Editorial = reader["Editorial"] as string,
                AnioPublicacion = reader["AnioPublicacion"] as short?,
                Categoria = reader["Categoria"] as string,
                Activo = (bool)reader["Activo"],
            };
        }
        else
        {
            material = new Tesis(id, isbn, titulo, autor, stockTotal, stockDisponible, (string)reader["Universidad"], reader["GradoAcademico"] as string, reader["Tutor"] as string)
            {
                Editorial = reader["Editorial"] as string,
                AnioPublicacion = reader["AnioPublicacion"] as short?,
                Categoria = reader["Categoria"] as string,
                Activo = (bool)reader["Activo"],
            };
        }

        return material;
    }
}
