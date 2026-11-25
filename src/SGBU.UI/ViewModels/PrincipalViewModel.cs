using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

public partial class PrincipalViewModel : ObservableObject
{
    private readonly IServicioVentanas ventanas;
    private readonly Servicios servicios;

    [ObservableProperty]
    private OpcionMenu? opcionSeleccionada;

    // Lo que se muestra a la derecha: el ViewModel de la pantalla elegida.
    // La ventana usa un DataTemplate para saber qué vista dibujar para cada ViewModel.
    [ObservableProperty]
    private object? vistaActual;

    public PrincipalViewModel(Sesion sesion, IServicioVentanas ventanas, Servicios servicios)
    {
        this.ventanas = ventanas;
        this.servicios = servicios;
        Sesion = sesion;
        Bienvenida = Textos.Texto("T_Bienvenido") + " " + sesion.NombreCompleto;

        // Solo se agregan al menú las opciones que el rol puede usar.
        Opciones = new ObservableCollection<OpcionMenu>();
        AgregarSiPuede("T_Menu_Catalogo", Permiso.VerCatalogo);
        AgregarSiPuede("T_Menu_MisPrestamos", Permiso.VerMisPrestamos);
        AgregarSiPuede("T_Menu_Prestamos", Permiso.RegistrarPrestamos);
        AgregarSiPuede("T_Menu_Usuarios", Permiso.GestionarUsuarios);
        AgregarSiPuede("T_Menu_Multas", Permiso.GestionarMultas);
        AgregarSiPuede("T_Menu_Reportes", Permiso.VerReportes);
        AgregarSiPuede("T_Menu_Auditoria", Permiso.VerAuditoria);
        AgregarSiPuede("T_Menu_Configuracion", Permiso.Preferencias);

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
            return;
        }

        switch (value.Permiso)
        {
            case Permiso.VerCatalogo:
                VistaActual = new CatalogoViewModel(servicios.Catalogo, Sesion);
                break;
            case Permiso.VerMisPrestamos:
                VistaActual = new MisPrestamosViewModel(servicios.Prestamos, servicios.Multas, Sesion);
                break;
            case Permiso.RegistrarPrestamos:
                VistaActual = new PrestamosViewModel(servicios.Prestamos, Sesion);
                break;
            case Permiso.GestionarUsuarios:
                VistaActual = new UsuariosViewModel(servicios.Usuarios, Sesion);
                break;
            case Permiso.GestionarMultas:
                VistaActual = new MultasViewModel(servicios.Multas, Sesion);
                break;
            case Permiso.VerReportes:
                VistaActual = new ReportesViewModel(servicios.Reportes, Sesion, ventanas);
                break;
            case Permiso.VerAuditoria:
                VistaActual = new AuditoriaViewModel(servicios.Auditoria, Sesion);
                break;
            default:
                VistaActual = new ConfiguracionViewModel(Sesion, ventanas);
                break;
        }
    }

    [RelayCommand]
    private void CerrarSesion()
    {
        ventanas.CerrarSesion();
    }

    private void AgregarSiPuede(string claveTitulo, Permiso permiso)
    {
        if (Sesion.Puede(permiso))
        {
            Opciones.Add(new OpcionMenu(Textos.Texto(claveTitulo), permiso));
        }
    }
}
