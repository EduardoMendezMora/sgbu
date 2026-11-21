using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Services.Catalogo;
using SGBU.Services.Seguridad;
using SGBU.Services.Usuarios;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

public partial class PrincipalViewModel : ObservableObject
{
    private readonly IServicioVentanas ventanas;
    private readonly ServicioCatalogo servicioCatalogo;
    private readonly ServicioUsuarios servicioUsuarios;

    [ObservableProperty]
    private OpcionMenu? opcionSeleccionada;

    // Lo que se muestra a la derecha: el ViewModel de la pantalla elegida.
    // La ventana usa un DataTemplate para saber qué vista dibujar para cada ViewModel.
    [ObservableProperty]
    private object? vistaActual;

    public PrincipalViewModel(Sesion sesion, IServicioVentanas ventanas, ServicioCatalogo servicioCatalogo, ServicioUsuarios servicioUsuarios)
    {
        this.ventanas = ventanas;
        this.servicioCatalogo = servicioCatalogo;
        this.servicioUsuarios = servicioUsuarios;
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

        OpcionSeleccionada = Opciones[0];
    }

    public Sesion Sesion { get; }

    public string Bienvenida { get; }

    public ObservableCollection<OpcionMenu> Opciones { get; }

    partial void OnOpcionSeleccionadaChanged(OpcionMenu? value)
    {
        if (value == null)
        {
            VistaActual = null;
        }
        else if (value.Permiso == Permiso.VerCatalogo)
        {
            VistaActual = new CatalogoViewModel(servicioCatalogo, Sesion);
        }
        else if (value.Permiso == Permiso.GestionarUsuarios)
        {
            VistaActual = new UsuariosViewModel(servicioUsuarios, Sesion);
        }
        else
        {
            VistaActual = value.Titulo + ": pantalla en construcción.";
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
