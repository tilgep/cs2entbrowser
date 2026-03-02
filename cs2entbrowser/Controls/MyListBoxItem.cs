using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Controls;

[PseudoClasses(":pressed", ":selected")]
public class MyListBoxItem : ContentControl, ISelectable
{
    /// <summary>
    /// Defines the <see cref="IsSelected"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<MyListBoxItem>();

    /// <summary>
    /// Initializes static members of the <see cref="MyListBoxItem"/> class.
    /// </summary>
    static MyListBoxItem()
    {
        SelectableMixin.Attach<MyListBoxItem>(IsSelectedProperty);
        PressedMixin.Attach<MyListBoxItem>();
        FocusableProperty.OverrideDefaultValue<MyListBoxItem>(true);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<MyListBoxItem>(IsOffscreenBehavior.FromClip);
    }

    /// <summary>
    /// Gets or sets the selection state of the item.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ListItemAutomationPeer(this);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        UpdateSelectionFromEvent(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        UpdateSelectionFromEvent(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        UpdateSelectionFromEvent(e);
    }

    protected bool UpdateSelectionFromEvent(RoutedEventArgs e) 
    {
        MyListBox? control = (MyListBox?)MyListBox.ItemsControlFromItemContainer(this);
        if (control == null)
            return false;

        return control.UpdateSelectionFromEvent(this);
    }
}
