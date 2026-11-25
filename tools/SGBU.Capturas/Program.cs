using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

// Uso: SGBU.Capturas <ruta de SGBU.UI.exe> <carpeta de salida>
string exe = args[0];
string carpeta = args[1];
Directory.CreateDirectory(carpeta);

Application app = Application.Launch(exe);
using UIA3Automation automatizacion = new UIA3Automation();

try
{
    // 1. Login vacío y con error
    Window login = app.GetMainWindow(automatizacion, TimeSpan.FromSeconds(60));
    Foto(login, "01_login.png");

    Entrar(login, "admin@sgbu.ac.cr", "clave-mala");
    Thread.Sleep(3000);
    Foto(login, "02_login_error.png");

    // 2. Administrador: catálogo, un material seleccionado, usuarios
    Entrar(login, "admin@sgbu.ac.cr", "Sgbu2026!");
    Window principal = EsperarVentana("Biblioteca Universitaria");
    principal.Patterns.Window.Pattern.SetWindowVisualState(FlaUI.Core.Definitions.WindowVisualState.Maximized);
    Thread.Sleep(3000);
    Foto(principal, "03_catalogo_admin.png");

    AutomationElement tabla = principal.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataGrid));
    if (tabla != null)
    {
        tabla.AsGrid().Select(0);
        Thread.Sleep(1000);
        Foto(principal, "04_catalogo_editar.png");
    }

    ElegirMenu(principal, "Usuarios");
    Thread.Sleep(3000);
    Foto(principal, "05_usuarios.png");

    ElegirMenu(principal, "Préstamos y devoluciones");
    Thread.Sleep(3000);
    Foto(principal, "06_prestamos.png");

    Escribir(principal.FindFirstDescendant(cf => cf.ByAutomationId("txtCarne")), "PRF-002");
    Escribir(principal.FindFirstDescendant(cf => cf.ByAutomationId("txtIsbn")), "9780134685991");
    principal.FindFirstDescendant(cf => cf.ByAutomationId("btnPrestar")).AsButton().Invoke();
    Thread.Sleep(3000);
    Foto(principal, "07_prestamo_registrado.png");

    // Devolución atrasada de Daniela (EST-1004, fila 2): genera una multa.
    SeleccionarFila(principal, 1);
    principal.FindFirstDescendant(cf => cf.ByAutomationId("btnDevolver")).AsButton().Invoke();
    Thread.Sleep(3000);
    Foto(principal, "08_devolucion_con_multa.png");

    ElegirMenu(principal, "Multas");
    Thread.Sleep(3000);
    Foto(principal, "09_multas.png");

    // Paga la multa de Daniela (fila 2) y deja la de María pendiente.
    SeleccionarFila(principal, 1);
    principal.FindFirstDescendant(cf => cf.ByAutomationId("btnPagar")).AsButton().Invoke();
    Thread.Sleep(3000);
    Foto(principal, "10_multa_pagada.png");

    ElegirMenu(principal, "Reportes");
    Thread.Sleep(4000);
    Foto(principal, "11_reporte_top10_con_grafico.png");

    principal.FindFirstDescendant(cf => cf.ByAutomationId("cmbReporte")).AsComboBox().Select(1);
    Thread.Sleep(3000);
    Foto(principal, "12_reporte_usuarios_con_multas.png");

    principal.FindFirstDescendant(cf => cf.ByAutomationId("cmbReporte")).AsComboBox().Select(2);
    Thread.Sleep(3000);
    Foto(principal, "13_reporte_stock_critico.png");

    ElegirMenu(principal, "Auditoría");
    Thread.Sleep(3000);
    Foto(principal, "14_auditoria.png");

    // 3. Estudiante: el menú tiene menos opciones y no hay formulario
    principal.FindFirstDescendant(cf => cf.ByAutomationId("btnCerrarSesion")).AsButton().Invoke();
    Thread.Sleep(2000);
    Window login2 = EsperarVentana("Iniciar sesión");
    Entrar(login2, "EST-1002", "Sgbu2026!");
    Window principalEstudiante = EsperarVentana("Biblioteca Universitaria");
    principalEstudiante.Patterns.Window.Pattern.SetWindowVisualState(FlaUI.Core.Definitions.WindowVisualState.Maximized);
    Thread.Sleep(3000);
    Foto(principalEstudiante, "15_catalogo_estudiante.png");

    ElegirMenu(principalEstudiante, "Mis préstamos");
    Thread.Sleep(3000);
    Foto(principalEstudiante, "16_mis_prestamos_y_multas.png");

    // 4. Configuración: tema oscuro e inglés. La ventana se vuelve a abrir con los cambios.
    ElegirMenu(principalEstudiante, "Configuración");
    Thread.Sleep(2000);
    Foto(principalEstudiante, "17_configuracion.png");
    principalEstudiante.FindFirstDescendant(cf => cf.ByAutomationId("cmbTema")).AsComboBox().Select(1);
    principalEstudiante.FindFirstDescendant(cf => cf.ByAutomationId("cmbIdioma")).AsComboBox().Select(1);
    principalEstudiante.FindFirstDescendant(cf => cf.ByAutomationId("btnAplicar")).AsButton().Invoke();
    Thread.Sleep(3000);
    Window enIngles = EsperarVentana("University Library");
    enIngles.Patterns.Window.Pattern.SetWindowVisualState(FlaUI.Core.Definitions.WindowVisualState.Maximized);
    Thread.Sleep(3000);
    Foto(enIngles, "18_tema_oscuro_ingles.png");

    Console.WriteLine("Capturas listas en " + carpeta);
}
finally
{
    app.Close();
    app.Dispose();
}

void Foto(Window ventana, string nombre)
{
    ventana.Focus();
    Thread.Sleep(500);
    Capture.Element(ventana).ToFile(Path.Combine(carpeta, nombre));
    Console.WriteLine("Foto: " + nombre);
}

void Entrar(Window ventana, string usuario, string clave)
{
    Escribir(ventana.FindFirstDescendant(cf => cf.ByAutomationId("txtUsuario")), usuario);
    Escribir(ventana.FindFirstDescendant(cf => cf.ByAutomationId("txtContrasena")), clave);
    ventana.FindFirstDescendant(cf => cf.ByAutomationId("btnIngresar")).AsButton().Invoke();
}

void Escribir(AutomationElement caja, string texto)
{
    caja.Focus();
    Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
    Keyboard.Type(VirtualKeyShort.DELETE);
    Keyboard.Type(texto);
}

void SeleccionarFila(Window ventana, int fila)
{
    AutomationElement grilla = ventana.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataGrid));
    grilla.AsGrid().Select(fila);
    Thread.Sleep(500);
}

void ElegirMenu(Window ventana, string opcion)
{
    AutomationElement menu = ventana.FindFirstDescendant(cf => cf.ByAutomationId("lstMenu"));
    menu.FindFirstDescendant(cf => cf.ByName(opcion)).AsListBoxItem().Select();
}

Window EsperarVentana(string parteDelTitulo)
{
    Window ventana = Retry.WhileNull(
        () => app.GetAllTopLevelWindows(automatizacion).FirstOrDefault(w => w.Title.Contains(parteDelTitulo) && w.IsAvailable),
        TimeSpan.FromSeconds(30)).Result;

    if (ventana == null)
    {
        throw new InvalidOperationException("No apareció la ventana: " + parteDelTitulo);
    }

    return ventana;
}
