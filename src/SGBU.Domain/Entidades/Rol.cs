using SGBU.Domain.Reglas;

namespace SGBU.Domain.Entidades;

/// <summary>Administrador, Profesor o Estudiante, con sus límites (tabla Roles).</summary>
public sealed class Rol
{
    public Rol(int id, string nombre, int limitePrestamos, int diasPrestamo)
    {
        Id = id;
        Nombre = Requerir.Texto(nombre, "El nombre del rol");
        LimitePrestamos = Requerir.Positivo(limitePrestamos, "El límite de préstamos");
        DiasPrestamo = Requerir.Positivo(diasPrestamo, "Los días de préstamo");
    }

    public int Id { get; }

    public string Nombre { get; }

    public int LimitePrestamos { get; }

    public int DiasPrestamo { get; }
}
