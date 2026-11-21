using SGBU.Domain.Entidades;

namespace SGBU.Services.Catalogo;

// Los datos que llegan del formulario de la pantalla Catálogo.
public class FormularioMaterial
{
    public int Id { get; set; }

    public TipoMaterial Tipo { get; set; } = TipoMaterial.Libro;

    public string Isbn { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Autor { get; set; } = string.Empty;

    public string? Editorial { get; set; }

    public short? AnioPublicacion { get; set; }

    public string? Categoria { get; set; }

    public int StockTotal { get; set; }

    public bool Activo { get; set; } = true;

    public string? Edicion { get; set; }

    public string? NumeroVolumen { get; set; }

    public string? Periodicidad { get; set; }

    public string? Universidad { get; set; }

    public string? GradoAcademico { get; set; }

    public string? Tutor { get; set; }

    // Llena el formulario con un material que ya existe (para editarlo).
    public static FormularioMaterial Desde(Material material)
    {
        FormularioMaterial datos = new FormularioMaterial();
        datos.Id = material.Id;
        datos.Tipo = material.Tipo;
        datos.Isbn = material.Isbn;
        datos.Titulo = material.Titulo;
        datos.Autor = material.Autor;
        datos.Editorial = material.Editorial;
        datos.AnioPublicacion = material.AnioPublicacion;
        datos.Categoria = material.Categoria;
        datos.StockTotal = material.StockTotal;
        datos.Activo = material.Activo;

        if (material is Libro libro)
        {
            datos.Edicion = libro.Edicion;
        }
        else if (material is Revista revista)
        {
            datos.NumeroVolumen = revista.NumeroVolumen;
            datos.Periodicidad = revista.Periodicidad;
        }
        else if (material is Tesis tesis)
        {
            datos.Universidad = tesis.Universidad;
            datos.GradoAcademico = tesis.GradoAcademico;
            datos.Tutor = tesis.Tutor;
        }

        return datos;
    }
}
