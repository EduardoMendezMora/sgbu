namespace SGBU.Services.Comun;

/// <summary>
/// La hora "de ahora" como dependencia. En la aplicación es la hora del sistema; en las
/// pruebas es una fecha fija, así el resultado no cambia según el día en que se corran.
/// </summary>
public interface IReloj
{
    DateTime Ahora { get; }
}
