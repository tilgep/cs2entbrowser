using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using cs2entbrowser.Controls;
using cs2entbrowser.Utils;
using cs2entbrowser.ViewModels;
using System.Diagnostics;

namespace cs2entbrowser.Views;

public partial class EntityBrowserView : UserControl
{
    private EntityBrowserViewModel _vm;
    public EntityBrowserView(LoadedVpk vpk)
    {
        InitializeComponent();

        _vm = new EntityBrowserViewModel(this, vpk);
        DataContext = _vm;
    }

    

    public string GetPath()
    {
        if (DataContext == null)
        {
            return "";
        }

        return ((EntityBrowserViewModel)DataContext).Path;
    }

    public void OutputTarget_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("double tabbed");
    }
}