using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Domain.Entidades;
using SGBU.Domain.Excepciones;
using SGBU.Domain.Repositorios;
using SGBU.Services.Catalogo;
using SGBU.Services.Seguridad;

namespace SGBU.UI.ViewModels;

public partial class CatalogoViewModel : ObservableObject
{
    private readonly ServicioCatalogo servicio;
    private readonly Sesion sesion;
    private int pagina = 1;
    private int totalPaginas = 1;
    private int idEditando;

    // ----- Búsqueda -----
    [ObservableProperty]
    private string textoBuscar = string.Empty;

    [ObservableProperty]
    private string tipoBuscar = "Todos";

    [ObservableProperty]
    private string infoPagina = string.Empty;

    [ObservableProperty]
    private Material? materialSeleccionado;

    [ObservableProperty]
    private string mensaje = string.Empty;

    // ----- Formulario -----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsLibro), nameof(EsRevista), nameof(EsTesis))]
    private TipoMaterial tipo = TipoMaterial.Libro;

    [ObservableProperty]
    private string isbn = string.Empty;

    [ObservableProperty]
    private string titulo = string.Empty;

    [ObservableProperty]
    private string autor = string.Empty;

    [ObservableProperty]
    private string editorial = string.Empty;

    [ObservableProperty]
    private string anio = string.Empty;

    [ObservableProperty]
    private string categoria = string.Empty;

    [ObservableProperty]
    private string stockTotal = "1";

    [ObservableProperty]
    private string edicion = string.Empty;

    [ObservableProperty]
    private string numeroVolumen = string.Empty;

    [ObservableProperty]
    private string periodicidad = string.Empty;

    [ObservableProperty]
    private string universidad = string.Empty;

    [ObservableProperty]
    private string gradoAcademico = string.Empty;

    [ObservableProperty]
    private string tutor = string.Empty;

    public CatalogoViewModel(ServicioCatalogo servicio, Sesion sesion)
    {
        this.servicio = servicio;
        this.sesion = sesion;
        PuedeEditar = sesion.Puede(Permiso.GestionarCatalogo);
        BuscarCommand.Execute(null);
    }

    public ObservableCollection<Material> Materiales { get; } = new ObservableCollection<Material>();

    public string[] TiposBuscar { get; } = { "Todos", "Libro", "Revista", "Tesis" };

    public TipoMaterial[] Tipos { get; } = { TipoMaterial.Libro, TipoMaterial.Revista, TipoMaterial.Tesis };

    // Solo el administrador ve el formulario.
    public bool PuedeEditar { get; }

    public bool EsLibro => Tipo == TipoMaterial.Libro;

    public bool EsRevista => Tipo == TipoMaterial.Revista;

    public bool EsTesis => Tipo == TipoMaterial.Tesis;

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
        MaterialSeleccionado = null;
        Tipo = TipoMaterial.Libro;
        Isbn = string.Empty;
        Titulo = string.Empty;
        Autor = string.Empty;
        Editorial = string.Empty;
        Anio = string.Empty;
        Categoria = string.Empty;
        StockTotal = "1";
        Edicion = string.Empty;
        NumeroVolumen = string.Empty;
        Periodicidad = string.Empty;
        Universidad = string.Empty;
        GradoAcademico = string.Empty;
        Tutor = string.Empty;
        Mensaje = string.Empty;
    }

    [RelayCommand]
    private async Task Guardar()
    {
        Mensaje = string.Empty;

        if (!int.TryParse(StockTotal, out int stock))
        {
            Mensaje = "El stock debe ser un número.";
            return;
        }

        short? anioNumero = null;
        if (!string.IsNullOrWhiteSpace(Anio))
        {
            if (!short.TryParse(Anio, out short valor))
            {
                Mensaje = "El año debe ser un número.";
                return;
            }

            anioNumero = valor;
        }

        FormularioMaterial datos = new FormularioMaterial();
        datos.Id = idEditando;
        datos.Tipo = Tipo;
        datos.Isbn = Isbn;
        datos.Titulo = Titulo;
        datos.Autor = Autor;
        datos.Editorial = Editorial;
        datos.AnioPublicacion = anioNumero;
        datos.Categoria = Categoria;
        datos.StockTotal = stock;
        datos.Edicion = Edicion;
        datos.NumeroVolumen = NumeroVolumen;
        datos.Periodicidad = Periodicidad;
        datos.Universidad = Universidad;
        datos.GradoAcademico = GradoAcademico;
        datos.Tutor = Tutor;
        if (MaterialSeleccionado != null)
        {
            datos.Activo = MaterialSeleccionado.Activo;
        }

        try
        {
            await servicio.GuardarAsync(sesion, datos);
            await CargarPagina();
            Nuevo();
            Mensaje = "Material guardado.";
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

    // Al seleccionar una fila de la tabla se llenan los campos del formulario.
    partial void OnMaterialSeleccionadoChanged(Material? value)
    {
        if (value == null)
        {
            return;
        }

        FormularioMaterial datos = FormularioMaterial.Desde(value);
        idEditando = datos.Id;
        Tipo = datos.Tipo;
        Isbn = datos.Isbn;
        Titulo = datos.Titulo;
        Autor = datos.Autor;
        Editorial = datos.Editorial ?? string.Empty;
        Anio = datos.AnioPublicacion?.ToString() ?? string.Empty;
        Categoria = datos.Categoria ?? string.Empty;
        StockTotal = datos.StockTotal.ToString();
        Edicion = datos.Edicion ?? string.Empty;
        NumeroVolumen = datos.NumeroVolumen ?? string.Empty;
        Periodicidad = datos.Periodicidad ?? string.Empty;
        Universidad = datos.Universidad ?? string.Empty;
        GradoAcademico = datos.GradoAcademico ?? string.Empty;
        Tutor = datos.Tutor ?? string.Empty;
        Mensaje = string.Empty;
    }

    private async Task CargarPagina()
    {
        TipoMaterial? tipoFiltro = null;
        if (TipoBuscar != "Todos")
        {
            tipoFiltro = Enum.Parse<TipoMaterial>(TipoBuscar);
        }

        try
        {
            Pagina<Material> resultado = await servicio.BuscarAsync(TextoBuscar, tipoFiltro, pagina);

            Materiales.Clear();
            foreach (Material material in resultado.Elementos)
            {
                Materiales.Add(material);
            }

            totalPaginas = Math.Max(1, resultado.TotalPaginas);
            InfoPagina = "Página " + pagina + " de " + totalPaginas + " (" + resultado.Total + " materiales)";
        }
        catch (Exception)
        {
            Mensaje = "No se pudo cargar el catálogo. Revise la conexión con la base de datos.";
        }
    }
}
