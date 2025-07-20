using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cs2entbrowser.ViewModels.Entity;

public class EntityPropertyViewModel : ViewModelBase
{
    private bool _matched = false;
    public bool Matched
    {
        get => _matched;
        set => this.RaiseAndSetIfChanged(ref _matched, value);
    }
    public string Key { get; private set; }
    public string Value { get; private set; }

    public EntityPropertyViewModel(string key, string value)
    {
        Key = key;
        Value = value;
    }
}

public class EntityOutputViewModel : ViewModelBase
{
    public string Output { get; private set; }
    public string Target { get; private set; }
    public string Input { get; private set; }
    public string Parameter { get; private set; }
    public float Delay { get; private set; }
    public int TimesToFire { get; private set; }

    public EntityOutputViewModel(string output, string target, string input, string parameter, float delay, int timesToFire)
    {
        Output = output;
        Target = target;
        Input = input;
        Parameter = parameter;
        Delay = delay;
        TimesToFire = timesToFire;
    }

    public bool BasicSearch(Regex search, string text, bool outputChain)
    {
        if (search.IsMatch(Output))
            return true;

        if (search.IsMatch(Target))
            return true;

        if (search.IsMatch(Input))
            return true;

        if (search.IsMatch(Parameter))
            return true;

        if (search.IsMatch(Delay.ToString()))
            return true;

        if (!outputChain)
            return false;

        // Search for output chain e.g. OnPressed>entname>Kill
        string[] parts = text.Split('>', 5);
        if(parts.Length <= 1)
            return false;

        switch(parts.Length)
        {
            case 2:
                if (Output.ToLower().Contains(parts[0]) &&
                   Target.ToLower().Contains(parts[1]))
                    return true;
                if (Target.ToLower().Contains(parts[0]) &&
                   Input.ToLower().Contains(parts[1]))
                    return true;
                if (Input.ToLower().Contains(parts[0]) &&
                   Parameter.ToLower().Contains(parts[1]))
                    return true;
                if (Parameter.ToLower().Contains(parts[0]) &&
                   Delay.ToString().Contains(parts[1]))
                    return true;
                break;
            case 3:
                if (Output.ToLower().Contains(parts[0]) &&
                   Target.ToLower().Contains(parts[1]) &&
                   Input.ToLower().Contains(parts[2]))
                    return true;
                if (Target.ToLower().Contains(parts[0]) &&
                   Input.ToLower().Contains(parts[1]) &&
                   Parameter.ToLower().Contains(parts[2]))
                    return true;
                if (Input.ToLower().Contains(parts[0]) &&
                   Parameter.ToLower().Contains(parts[1]) &&
                   Delay.ToString().Contains(parts[2]))
                    return true;
                break;
            case 4:
                if (Output.ToLower().Contains(parts[0]) &&
                   Target.ToLower().Contains(parts[1]) &&
                   Input.ToLower().Contains(parts[2]) &&
                   Parameter.ToLower().Contains(parts[3]))
                    return true;
                if (Target.ToLower().Contains(parts[0]) &&
                   Input.ToLower().Contains(parts[1]) &&
                   Parameter.ToLower().Contains(parts[2]) &&
                   Delay.ToString().Contains(parts[3]))
                    return true;
                break;
            case 5:
                if (Output.ToLower().Contains(parts[0]) &&
                   Target.ToLower().Contains(parts[1]) &&
                   Input.ToLower().Contains(parts[2]) &&
                   Parameter.ToLower().Contains(parts[3]) &&
                   Delay.ToString().Contains(parts[4]))
                    return true;
                break;
        }

        return false;
    }
}
