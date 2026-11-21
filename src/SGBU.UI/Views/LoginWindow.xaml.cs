using System.Windows;
using SGBU.UI.ViewModels;

namespace SGBU.UI.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    // El PasswordBox no se puede enlazar con Binding, así que se copia la clave al ViewModel.
    private void Contrasena_Cambio(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.Contrasena = txtContrasena.Password;
        }
    }
}
