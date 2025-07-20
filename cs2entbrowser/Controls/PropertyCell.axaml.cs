using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using cs2entbrowser.Services;
using ReactiveUI;
using System.Data.Common;
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;

namespace cs2entbrowser.Controls;

public class CellEvent
{
    string Text;
    public CellEvent(string text)
    {
        Text = text;
    }
}

public partial class PropertyCell : UserControl
{
    public static readonly StyledProperty<string> TargetProperty = AvaloniaProperty.Register<PropertyCell, string>(
        nameof(Target),
        defaultValue: string.Empty,
        defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
    );

    public string Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    private SearchTarget _searchTarget;

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<PropertyCell, string>(
        nameof(Text),
        defaultValue: string.Empty,
        defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
    );

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public PropertyCell()
    {
        InitializeComponent();
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        TopLevel? tl = TopLevel.GetTopLevel(topPanel);
        if (tl != null && tl.Clipboard != null)
        {
            tl.Clipboard.SetTextAsync(Text);
        }
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        VpkService.Instance.RequestSearch(SearchType.Search, GetTarget(), Text);
    }

    private void Jump_Click(object sender, RoutedEventArgs e)
    {
        VpkService.Instance.RequestSearch(SearchType.Jump, _searchTarget, Text);
    }

    private void DoubleClicked(object sender, RoutedEventArgs e)
    {
        if (SettingsService.Instance.DoubleClickBehaviour == DoubleClickBehaviour.Jump)
            VpkService.Instance.RequestSearch(SearchType.Jump, GetTarget(), Text);
        else if (SettingsService.Instance.DoubleClickBehaviour == DoubleClickBehaviour.Search)
            VpkService.Instance.RequestSearch(SearchType.Search, GetTarget(), Text);
    }

    private SearchTarget GetTarget()
    {
        Debug.WriteLine("cell target:" + Target);
        if (Target.ToLower() == "key")
            return SearchTarget.Key;
        else if (Target.ToLower() == "output")
            return SearchTarget.Output;
        else
            return SearchTarget.Value;
    }
}