using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

/// <summary>
/// Ventana principal: muestra solo las opciones que el rol del usuario puede usar.
/// Las pantallas de cada opción llegan en los pasos 6 a 10.
/// </summary>
public sealed partial class PrincipalViewModel : ObservableObject
{
    private static readonly OpcionMenu[] TodasLasOpciones =
    [
        new("Catálogo", Permiso.VerCatalogo),
        new("Mis préstamos", Permiso.VerMisPrestamos),
        new("Préstamos y devoluciones", Permiso.RegistrarPrestamos),
        new("Usuarios", Permiso.GestionarUsuarios),
        new("Multas", Permiso.GestionarMultas),
        new("Reportes", Permiso.VerReportes),
        new("Auditoría", Permiso.VerAuditoria),
        new("Configuración", Permiso.Configurar),
    ];

    private readonly IServicioVentanas ventanas;

    [ObservableProperty]
    private OpcionMenu? opcionSeleccionada;

    public PrincipalViewModel(Sesion sesion, IServicioVentanas ventanas)
    {
        Sesion = sesion;
        this.ventanas = ventanas;
        Opciones = new ObservableCollection<OpcionMenu>(TodasLasOpciones.Where(o => sesion.Puede(o.Permiso)));
        OpcionSeleccionada = Opciones.FirstOrDefault();
    }

    public Sesion Sesion { get; }

    public string Bienvenida => $"Bienvenido(a), {Sesion.NombreCompleto}";

    public ObservableCollection<OpcionMenu> Opciones { get; }

    public string Contenido => OpcionSeleccionada is null
        ? string.Empty
        : $"{OpcionSeleccionada.Titulo}: esta pantalla se construye en los próximos pasos.";

    partial void OnOpcionSeleccionadaChanged(OpcionMenu? value) => OnPropertyChanged(nameof(Contenido));

    [RelayCommand]
    private void CerrarSesion() => ventanas.CerrarSesion();
}
