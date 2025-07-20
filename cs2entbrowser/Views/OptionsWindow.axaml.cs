using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using cs2entbrowser.ViewModels;

namespace cs2entbrowser.Views;

public partial class OptionsWindow : Window
{
    private OptionsWindowViewModel _vm;
    public OptionsWindow()
    {
        InitializeComponent();

        _vm = new OptionsWindowViewModel(this);
        DataContext = _vm;
    }
}