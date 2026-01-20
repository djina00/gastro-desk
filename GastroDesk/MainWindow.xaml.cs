using System.Windows;
using GastroDesk.Data;
using GastroDesk.ViewModels;

namespace GastroDesk;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Initialize database
        DbContextFactory.Instance.EnsureDatabaseCreated();

        // Set DataContext
        DataContext = new MainViewModel();
    }
}
