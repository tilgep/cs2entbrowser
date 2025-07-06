using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace cs2entbrowser.Controls;

public partial class ClearableTextBox : UserControl
{
    public ClearableTextBox()
    {
        InitializeComponent();
    }

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

    public static readonly StyledProperty<string> WatermarkProperty = AvaloniaProperty.Register<
        ClearableTextBox,
        string
    >(nameof(Watermark));

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public static readonly StyledProperty<string> CornerRadiusPropertyA = AvaloniaProperty.Register<
        ClearableTextBox,
        string
    >(nameof(CornerRadiusA));

    public string CornerRadiusA
    {
        get => GetValue(CornerRadiusPropertyA);
        set => SetValue(CornerRadiusPropertyA, value);
    }

    private void Panel_PointerEntered(object? sender, RoutedEventArgs e)
    {
        ClearButton.IsVisible = true;
    }

    private void Panel_PointerExited(object? sender, RoutedEventArgs e)
    {
        ClearButton.IsVisible = false;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Text = "";
    }
}