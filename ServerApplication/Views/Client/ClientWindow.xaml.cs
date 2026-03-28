using System.Windows;
using ServerApplication.ViewModels;

namespace ServerApplication.Views.Client;

public partial class ClientWindow : Window
{
    public ClientWindow()
    {
        InitializeComponent();
        // ViewModel is set in XAML, but can also be accessed here if needed.
    }
}