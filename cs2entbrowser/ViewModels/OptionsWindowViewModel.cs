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
    
    private string _output = "";
    private string _target = "";
    private string _input = "";
    private string _param = "";
    private string _delay = "";
    private string _ttf = "";
    private string _exampleText = "";
    private bool _canSave = true;
    public string Output
    {
        get => _output;
        set => this.RaiseAndSetIfChanged(ref _output, value);
    }
    public string Target
    {
        get => _target;
        set => this.RaiseAndSetIfChanged(ref _target, value);
    }
    public string Input
    {
        get => _input;
        set => this.RaiseAndSetIfChanged(ref _input, value);
    }
    public string Param
    {
        get => _param;
        set => this.RaiseAndSetIfChanged(ref _param, value);
    }
    public string Delay
    {
        get => _delay;
        set => this.RaiseAndSetIfChanged(ref _delay, value);
    }
    public string TTF
    {
        get => _ttf;
        set => this.RaiseAndSetIfChanged(ref _ttf, value);
    }
    public string ExampleText
    {
        get => _exampleText;
        set => this.RaiseAndSetIfChanged(ref _exampleText, value);
    }
    public bool CanSave
    {
        get => _canSave;
        set => this.RaiseAndSetIfChanged(ref _canSave, value);
    }

    public OptionsWindowViewModel() { }
    public OptionsWindowViewModel(OptionsWindow optionsWindow)
    {
        OptionsWindow = optionsWindow;

        if (SettingsService.Instance.Loaded)
        {
            GetSettings();
        }

        this.WhenAnyValue(x => x.Output)
            .Subscribe(_ => UpdateExampleText());
        this.WhenAnyValue(x => x.Target)
            .Subscribe(_ => UpdateExampleText());
        this.WhenAnyValue(x => x.Input)
            .Subscribe(_ => UpdateExampleText());
        this.WhenAnyValue(x => x.Param)
            .Subscribe(_ => UpdateExampleText());
        this.WhenAnyValue(x => x.Delay)
            .Subscribe(_ => UpdateExampleText());
        this.WhenAnyValue(x => x.TTF)
            .Subscribe(_ => UpdateExampleText());

        SettingsService.Instance.WhenAnyValue(x => x.Loaded)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SettingsLoadedStateChanged());
    }

    void GetSettings()
    {
        Output = SettingsService.Instance.IOOutput;
        Target = SettingsService.Instance.IOTarget;
        Input = SettingsService.Instance.IOInput;
        Param = SettingsService.Instance.IOParam;
        Delay = SettingsService.Instance.IODelay;
        TTF = SettingsService.Instance.IOTTF;

        UpdateExampleText();
    }

    void SettingsLoadedStateChanged()
    {
        if (SettingsService.Instance.Loaded)
        {
            GetSettings();
        }
    }

    public void UpdateExampleText()
    {
        ExampleText = "{\n";
        ExampleText += $"    \"{Output}\": \"OnStartTouch\",\n";
        ExampleText += $"    \"{Target}\": \"server\",\n";
        ExampleText += $"    \"{Input}\": \"Command\",\n";
        ExampleText += $"    \"{Param}\": \"say Made by tilgep\",\n";
        ExampleText += $"    \"{Delay}\": 10,\n";
        ExampleText += $"    \"{TTF}\": -1,\n";
        ExampleText += "}";
        CanSave = true;
    }
    

    public void SaveButton()
    {
        SettingsService.Instance.SetIO(Output, Target, Input, Param, Delay, TTF);
        CanSave = false;
    }
}
