using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Seguridad;
using SGBU.Services.Usuarios;

namespace SGBU.UI.ViewModels;

public partial class UsuariosViewModel : ObservableObject
{
    private readonly ServicioUsuarios servicio;
    private readonly Sesion sesion;
    private int pagina = 1;
    private int totalPaginas = 1;
    private int idEditando;

    [ObservableProperty]
    private string textoBuscar = string.Empty;

    [ObservableProperty]
    private string infoPagina = string.Empty;

    [ObservableProperty]
    private Usuario? usuarioSeleccionado;

    [ObservableProperty]
    private string mensaje = string.Empty;

    // ----- Formulario -----
    [ObservableProperty]
    private string carne = string.Empty;

    [ObservableProperty]
    private string dni = string.Empty;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string apellidos = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string telefono = string.Empty;

    [ObservableProperty]
    private int rolId;

    [ObservableProperty]
    private string contrasena = string.Empty;

    [ObservableProperty]
    private bool activo = true;

    public UsuariosViewModel(ServicioUsuarios servicio, Sesion sesion)
    {
        this.servicio = servicio;
        this.sesion = sesion;
        CargarRolesCommand.Execute(null);
        BuscarCommand.Execute(null);
    }

    public ObservableCollection<Usuario> Usuarios { get; } = new ObservableCollection<Usuario>();

    public ObservableCollection<Rol> Roles { get; } = new ObservableCollection<Rol>();

    [RelayCommand]
    private async Task CargarRoles()
    {
        try
        {
            foreach (Rol rol in await servicio.ListarRolesAsync())
            {
                Roles.Add(rol);
            }
        }
        catch (Exception)
        {
            Mensaje = "No se pudieron cargar los roles.";
        }
    }

    [RelayCommand]
    private async Task Buscar()
    {
        pagina = 1;
        await CargarPagina();
    }

    [RelayCommand]
    private async Task Anterior()
    {
        if (pagina > 1)
        {
            pagina--;
            await CargarPagina();
        }
    }

    [RelayCommand]
    private async Task Siguiente()
    {
        if (pagina < totalPaginas)
        {
            pagina++;
            await CargarPagina();
        }
    }

    [RelayCommand]
    private void Nuevo()
    {
        idEditando = 0;
        UsuarioSeleccionado = null;
        Carne = string.Empty;
        Dni = string.Empty;
        Nombre = string.Empty;
        Apellidos = string.Empty;
        Email = string.Empty;
        Telefono = string.Empty;
        RolId = 0;
        Contrasena = string.Empty;
        Activo = true;
        Mensaje = string.Empty;
    }

    [RelayCommand]
    private async Task Guardar()
    {
        FormularioUsuario datos = new FormularioUsuario();
        datos.Id = idEditando;
        datos.Carne = Carne;
        datos.Dni = Dni;
        datos.Nombre = Nombre;
        datos.Apellidos = Apellidos;
        datos.Email = Email;
        datos.Telefono = Telefono;
        datos.RolId = RolId;
        datos.Contrasena = Contrasena;
        datos.Activo = Activo;

        try
        {
            await servicio.GuardarAsync(sesion, datos);
            await CargarPagina();
            Nuevo();
            Mensaje = "Usuario guardado.";
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo guardar. Revise la conexión con la base de datos.";
        }
    }

    partial void OnUsuarioSeleccionadoChanged(Usuario? value)
    {
        if (value == null)
        {
            return;
        }

        idEditando = value.Id;
        Carne = value.Carne;
        Dni = value.Dni;
        Nombre = value.Nombre;
        Apellidos = value.Apellidos;
        Email = value.Email;
        Telefono = value.Telefono ?? string.Empty;
        RolId = value.Rol.Id;
        Contrasena = string.Empty;
        Activo = value.Activo;
        Mensaje = string.Empty;
    }

    private async Task CargarPagina()
    {
        try
        {
            Pagina<Usuario> resultado = await servicio.BuscarAsync(sesion, TextoBuscar, pagina);

            Usuarios.Clear();
            foreach (Usuario usuario in resultado.Elementos)
            {
                Usuarios.Add(usuario);
            }

            totalPaginas = Math.Max(1, resultado.TotalPaginas);
            InfoPagina = Textos.Texto("T_Pagina") + " " + pagina + " " + Textos.Texto("T_De") + " " + totalPaginas + " (" + resultado.Total + " " + Textos.Texto("T_Usuarios") + ")";
        }
        catch (ExcepcionDominio ex)
        {
            Mensaje = ex.Message;
        }
        catch (Exception)
        {
            Mensaje = "No se pudo cargar la lista. Revise la conexión con la base de datos.";
        }
    }
}
