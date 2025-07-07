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
    public static readonly StyledProperty<string> _propSearchText = AvaloniaProperty.Register<CopyableTextBox, string>(
        nameof(PropSearchText),
        defaultValue: string.Empty,
        defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
    );

    public string PropSearchText
    {
        get => GetValue(_propSearchText);
        set => SetValue(_propSearchText, value);
    }

    private EntityBrowserViewModel _vm;
    public EntityBrowserView(LoadedVpk vpk)
    {
        InitializeComponent();

        _vm = new EntityBrowserViewModel(this, vpk);
        DataContext = _vm;
    }

    public void PropertySearchText_TextInput(object? sender, RoutedEventArgs e)
    {
        PropSearchText = PropertySearch_TextBox.Text.Trim();
    }
}