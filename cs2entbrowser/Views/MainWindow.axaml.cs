using Avalonia.Controls;
using cs2entbrowser.ViewModels;

namespace cs2entbrowser.Views;

public partial class MainWindow : Window
{
    private MainViewModel _vm;
    public MainWindow()
    {
        InitializeComponent();

        _vm = new MainViewModel(this);
        DataContext = _vm;
    }
}
