using cs2entbrowser.Services;
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

public class OptionsWindowViewModel : ViewModelBase
{
    public OptionsWindow OptionsWindow { get; private set; }

    private bool dblClkNone = false;
    private bool dblClkJump = false;
    private bool dblClkSearch = false;

    public bool DblClkNone
    {
        get => dblClkNone;
        set => this.RaiseAndSetIfChanged(ref dblClkNone, value);
    }
    public bool DblClkJump
    {
        get => dblClkJump;
        set => this.RaiseAndSetIfChanged(ref dblClkJump, value);
    }
    public bool DblClkSearch
    {
        get => dblClkSearch;
        set => this.RaiseAndSetIfChanged(ref dblClkSearch, value);
    }

    public OptionsWindowViewModel() { }
    public OptionsWindowViewModel(OptionsWindow optionsWindow)
    {
        OptionsWindow = optionsWindow;

        if (SettingsService.Instance.Loaded)
        {
            GetSettings();
        }

        this.WhenAnyValue(x => x.DblClkNone)
            .Subscribe(_ => DoubleClickNoneChanged());
        this.WhenAnyValue(x => x.DblClkJump)
            .Subscribe(_ => DoubleClickJumpChanged());
        this.WhenAnyValue(x => x.DblClkSearch)
            .Subscribe(_ => DoubleClickSearchChanged());

        SettingsService.Instance.WhenAnyValue(x => x.Loaded)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SettingsLoadedStateChanged());
    }

    void GetSettings()
    {
        DoubleClickBehaviour behaviour = SettingsService.Instance.DoubleClickBehaviour;
        switch (behaviour)
        {
            case DoubleClickBehaviour.None:
                SetDoubleClickNone(); break;
            case DoubleClickBehaviour.Jump:
                SetDoubleClickJump(); break;
            case DoubleClickBehaviour.Search:
                SetDoubleClickSearch(); break;
        }
    }

    void SettingsLoadedStateChanged()
    {
        if (SettingsService.Instance.Loaded)
        {
            GetSettings();
        }
    }

    private void DoubleClickNoneChanged()
    {
        if (DblClkNone)
            SetDoubleClickNone();
    }
    private void DoubleClickJumpChanged()
    {
        if (DblClkJump)
            SetDoubleClickJump();
    }
    private void DoubleClickSearchChanged()
    {
        if (DblClkSearch)
            SetDoubleClickSearch();
    }
    private void SetDoubleClickNone()
    {
        DblClkNone = true;
        DblClkJump = false;
        DblClkSearch = false;
        SettingsService.Instance.DoubleClickBehaviour = DoubleClickBehaviour.None;
        SettingsService.Instance.WriteSettings();
    }
    private void SetDoubleClickJump()
    {
        DblClkNone = false;
        DblClkJump = true;
        DblClkSearch = false;
        SettingsService.Instance.DoubleClickBehaviour = DoubleClickBehaviour.Jump;
        SettingsService.Instance.WriteSettings();
    }
    private void SetDoubleClickSearch()
    {
        DblClkNone = false;
        DblClkJump = false;
        DblClkSearch = true;
        SettingsService.Instance.DoubleClickBehaviour = DoubleClickBehaviour.Search;
        SettingsService.Instance.WriteSettings();
    }
}
