using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace cs2entbrowser.Controls;

public partial class CopyableTextBox : UserControl
{
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<CopyableTextBox, string>(
        nameof(Text),
        defaultValue: string.Empty,
        defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
    );

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public CopyableTextBox()
    {
        InitializeComponent();
    }

    private void Panel_PointerEntered(object? sender, RoutedEventArgs e)
    {
        CopyButton.IsVisible = true;
    }

    private void Panel_PointerExited(object? sender, RoutedEventArgs e)
    {
        CopyButton.IsVisible = false;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        TopLevel? tl = TopLevel.GetTopLevel(CopyButton);
        if(tl != null && tl.Clipboard != null)
        {
            tl.Clipboard.SetTextAsync(Text);
        }
    }
}