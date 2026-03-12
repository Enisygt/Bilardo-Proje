using System.Windows;
using ServerApplication.ViewModels;

namespace ServerApplication;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}