using System.Windows;
using SGBU.UI.ViewModels;

namespace SGBU.UI.Views;

/// <summary>
/// Code-behind mínimo: solo une la vista con su ViewModel y copia la contraseña
/// (el PasswordBox no admite Binding). Ninguna regla de negocio vive aquí.
/// </summary>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = viewModel;
    }

    private void AlCambiarContrasena(object sender, RoutedEventArgs e) => viewModel.Contrasena = CajaContrasena.Password;
}
