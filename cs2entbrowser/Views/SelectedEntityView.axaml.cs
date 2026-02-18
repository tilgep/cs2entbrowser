using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using cs2entbrowser.Controls;
using cs2entbrowser.ViewModels;
using cs2entbrowser.ViewModels.Entity;
using ReactiveUI;
using System.Diagnostics;

namespace cs2entbrowser.Views;

public partial class SelectedEntityView : UserControl
{
    public static readonly StyledProperty<string> _propSearchText = AvaloniaProperty.Register<SelectedEntityView, string>(
        nameof(PropSearchText),
        defaultValue: string.Empty,
        defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
    );

    public string PropSearchText
    {
        get => GetValue(_propSearchText);
        set => SetValue(_propSearchText, value);
    }

    // Define the property with your specific EntityViewModel type
    public static readonly StyledProperty<EntityViewModel> SelectedEntityProperty =
        AvaloniaProperty.Register<SelectedEntityView, EntityViewModel>(
            nameof(SelectedEntity),
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public EntityViewModel SelectedEntity
    {
        get => GetValue(SelectedEntityProperty);
        set
        {
            System.Diagnostics.Debug.WriteLine($"SelectedEntity SET: {value?.Classname ?? "null"}");
            SetValue(SelectedEntityProperty, value);
        }
    }

    //private SelectedEntityViewModel vm;
    public SelectedEntityView()
    {
        InitializeComponent();
    }

    public void PropertySearchText_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e)
    {
        PropSearchText = PropertySearch_TextBox.Text.Trim();
    }
}