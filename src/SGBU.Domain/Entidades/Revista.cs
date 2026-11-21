using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

public sealed class Revista : Material
{
    public Revista(string isbn, string titulo, string autor, int stockTotal, string? numeroVolumen, string? periodicidad)
        : this(0, isbn, titulo, autor, stockTotal, stockTotal, numeroVolumen, periodicidad)
    {
    }

    public Revista(int id, string isbn, string titulo, string autor, int stockTotal, int stockDisponible, string? numeroVolumen, string? periodicidad)
        : base(id, isbn, titulo, autor, stockTotal, stockDisponible)
    {
        NumeroVolumen = Requerir.TextoOpcional(numeroVolumen);
        Periodicidad = Requerir.TextoOpcional(periodicidad);
    }

    public override TipoMaterial Tipo => TipoMaterial.Revista;

    public string? NumeroVolumen { get; }

    public string? Periodicidad { get; }

    public override string Detalle => string.Join(" · ", new[] { NumeroVolumen, Periodicidad }.Where(t => t is not null));
}
