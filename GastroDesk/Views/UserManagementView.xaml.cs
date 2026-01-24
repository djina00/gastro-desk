using System.Windows;
using System.Windows.Controls;
using GastroDesk.ViewModels;

namespace GastroDesk.Views
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.NewPassword = PasswordBox.Password;
            }
        }
    }
}
