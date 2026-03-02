using cs2entbrowser.Services;
using cs2entbrowser.ViewModels.Entity;
using cs2entbrowser.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.ViewModels;

public class SelectedEntityViewModel : ViewModelBase
{
    private SelectedEntityView View;
    public EntityViewModel Entity { get; private set; }

    public SelectedEntityViewModel(SelectedEntityView view)
    {
        View = view;

        if (SettingsService.Instance.Loaded)
        {
            ShowRawProperties = SettingsService.Instance.RawProperties;
        }

        //SettingsService.Instance.WhenAnyValue(x => x.RawProperties)
          //  .ObserveOn(RxApp.MainThreadScheduler)
            //.Subscribe(_ => RawPropertiesChanged());
    }

    private string _propertySearchText = "";
    public string PropertySearchText
    {
        get => _propertySearchText;
        set => this.RaiseAndSetIfChanged(ref _propertySearchText, value);
    }

    public void PropertySearch()
    {
        Debug.WriteLine("Searching properties for: " + PropertySearchText);

        if (Entity != null)
            Entity.FilterProperties(PropertySearchText.Trim().ToLower());
    }

    private bool _showRawProperties = false;
    public bool ShowRawProperties
    {
        get => _showRawProperties;
        set => this.RaiseAndSetIfChanged(ref _showRawProperties, value);
    }

    public void RawPropertiesChanged()
    {
        ShowRawProperties = SettingsService.Instance.RawProperties;
    }
}
