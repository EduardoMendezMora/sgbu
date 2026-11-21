using System.Windows;
using System.Windows.Controls;
using SGBU.UI.ViewModels;

namespace SGBU.UI.Views;

public partial class UsuariosView : UserControl
{
    public UsuariosView()
    {
        InitializeComponent();
    }

    // El PasswordBox no se puede enlazar con Binding, así que se copia la clave al ViewModel.
    private void Contrasena_Cambio(object sender, RoutedEventArgs e)
    {
        if (DataContext is UsuariosViewModel vm)
        {
            vm.Contrasena = txtContrasena.Password;
        }
    }

    private void Limpiar_Click(object sender, RoutedEventArgs e)
    {
        txtContrasena.Clear();
    }
}
