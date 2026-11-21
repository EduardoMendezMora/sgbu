using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

public class Tesis : Material
{
    public Tesis(string isbn, string titulo, string autor, int stockTotal, string universidad, string? gradoAcademico, string? tutor)
        : this(0, isbn, titulo, autor, stockTotal, stockTotal, universidad, gradoAcademico, tutor)
    {
    }

    public Tesis(int id, string isbn, string titulo, string autor, int stockTotal, int stockDisponible, string universidad, string? gradoAcademico, string? tutor)
        : base(id, isbn, titulo, autor, stockTotal, stockDisponible)
    {
        Universidad = Requerir.Texto(universidad, "La universidad de la tesis");
        GradoAcademico = Requerir.TextoOpcional(gradoAcademico);
        Tutor = Requerir.TextoOpcional(tutor);
    }

    public override TipoMaterial Tipo => TipoMaterial.Tesis;

    public string Universidad { get; }

    public string? GradoAcademico { get; }

    public string? Tutor { get; }

    public override string Detalle => GradoAcademico is null ? Universidad : $"{GradoAcademico} · {Universidad}";
}
