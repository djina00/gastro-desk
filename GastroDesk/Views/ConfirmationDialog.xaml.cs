using System.Windows;

namespace GastroDesk.Views
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string title, string message, string confirmButtonText = "Delete")
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
            ConfirmButton.Content = confirmButtonText;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static bool Show(string title, string message, string confirmButtonText = "Delete")
        {
            var dialog = new ConfirmationDialog(title, message, confirmButtonText);
            dialog.Owner = Application.Current.MainWindow;
            return dialog.ShowDialog() == true;
        }
    }
}
