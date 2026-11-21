using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

public sealed class Libro : Material
{
    /// <summary>Libro nuevo: todos sus ejemplares empiezan disponibles.</summary>
    public Libro(string isbn, string titulo, string autor, int stockTotal, string? edicion = null)
        : this(0, isbn, titulo, autor, stockTotal, stockTotal, edicion)
    {
    }

    /// <summary>Libro leído de la base de datos, con su Id y su stock real.</summary>
    public Libro(int id, string isbn, string titulo, string autor, int stockTotal, int stockDisponible, string? edicion)
        : base(id, isbn, titulo, autor, stockTotal, stockDisponible)
    {
        Edicion = Requerir.TextoOpcional(edicion);
    }

    public override TipoMaterial Tipo => TipoMaterial.Libro;

    public string? Edicion { get; }

    public override string Detalle => Edicion is null ? "Sin edición indicada" : $"Edición {Edicion}";
}
