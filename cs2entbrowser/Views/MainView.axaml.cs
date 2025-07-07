using Avalonia.Controls;
using Avalonia.Platform.Storage;
using cs2entbrowser.Services;
using cs2entbrowser.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace cs2entbrowser.Views;

public partial class MainView : UserControl
{
    private MainViewModel _vm;
    public MainView()
    {
        InitializeComponent();

        _vm = new MainViewModel(this);
        DataContext = _vm;
    }
}
