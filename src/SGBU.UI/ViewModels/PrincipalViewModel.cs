using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

public partial class PrincipalViewModel : ObservableObject
{
    private readonly IServicioVentanas ventanas;

    [ObservableProperty]
    private OpcionMenu? opcionSeleccionada;

    [ObservableProperty]
    private string contenido = string.Empty;

    public PrincipalViewModel(Sesion sesion, IServicioVentanas ventanas)
    {
        this.ventanas = ventanas;
        Sesion = sesion;
        Bienvenida = "Bienvenido(a), " + sesion.NombreCompleto;

        // Solo se agregan al menú las opciones que el rol puede usar.
        Opciones = new ObservableCollection<OpcionMenu>();
        AgregarSiPuede("Catálogo", Permiso.VerCatalogo);
        AgregarSiPuede("Mis préstamos", Permiso.VerMisPrestamos);
        AgregarSiPuede("Préstamos y devoluciones", Permiso.RegistrarPrestamos);
        AgregarSiPuede("Usuarios", Permiso.GestionarUsuarios);
        AgregarSiPuede("Multas", Permiso.GestionarMultas);
        AgregarSiPuede("Reportes", Permiso.VerReportes);
        AgregarSiPuede("Auditoría", Permiso.VerAuditoria);
        AgregarSiPuede("Configuración", Permiso.Configurar);
    }

    public Sesion Sesion { get; }

    public string Bienvenida { get; }

    public ObservableCollection<OpcionMenu> Opciones { get; }

    // Se ejecuta solo cada vez que cambia la opción seleccionada.
    partial void OnOpcionSeleccionadaChanged(OpcionMenu? value)
    {
        if (value == null)
        {
            Contenido = string.Empty;
        }
        else
        {
            Contenido = value.Titulo + ": pantalla en construcción.";
        }
    }

    [RelayCommand]
    private void CerrarSesion()
    {
        ventanas.CerrarSesion();
    }

    private void AgregarSiPuede(string titulo, Permiso permiso)
    {
        if (Sesion.Puede(permiso))
        {
            Opciones.Add(new OpcionMenu(titulo, permiso));
        }
    }
}
