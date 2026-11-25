using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGBU.Services.Configuracion;
using SGBU.Services.Seguridad;
using SGBU.UI.Navegacion;

namespace SGBU.UI.ViewModels;

public partial class ConfiguracionViewModel : ObservableObject
{
    private readonly Sesion sesion;
    private readonly IServicioVentanas ventanas;

    [ObservableProperty]
    private Opcion? temaSeleccionado;

    [ObservableProperty]
    private Opcion? idiomaSeleccionado;

    public ConfiguracionViewModel(Sesion sesion, IServicioVentanas ventanas)
    {
        this.sesion = sesion;
        this.ventanas = ventanas;

        Temas = new List<Opcion>();
        Temas.Add(new Opcion("Claro", Textos.Texto("T_Claro")));
        Temas.Add(new Opcion("Oscuro", Textos.Texto("T_Oscuro")));

        Idiomas = new List<Opcion>();
        Idiomas.Add(new Opcion("es", "Español"));
        Idiomas.Add(new Opcion("en", "English"));

        // Se marcan los valores que están guardados.
        Preferencias actuales = Preferencias.Cargar(Preferencias.RutaPorDefecto());
        TemaSeleccionado = Temas.Find(t => t.Valor == actuales.Tema);
        IdiomaSeleccionado = Idiomas.Find(i => i.Valor == actuales.Idioma);
    }

    public List<Opcion> Temas { get; }

    public List<Opcion> Idiomas { get; }

    [RelayCommand]
    private void Guardar()
    {
        Preferencias preferencias = new Preferencias();
        preferencias.Tema = TemaSeleccionado?.Valor ?? "Claro";
        preferencias.Idioma = IdiomaSeleccionado?.Valor ?? "es";
        preferencias.Guardar(Preferencias.RutaPorDefecto());

        // Se cargan los nuevos colores y textos, y se vuelve a abrir la ventana para verlos.
        Apariencia.Aplicar(preferencias);
        ventanas.MostrarPrincipal(sesion);
    }
}
