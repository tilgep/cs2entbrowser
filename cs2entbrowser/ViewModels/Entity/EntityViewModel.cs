using cs2entbrowser.Utils;
using cs2entbrowser.Utils.Parser.KV3;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.ViewModels.Entity;

/// <summary>
/// Represents an entity parsed from an entity lump
/// </summary>
public class EntityViewModel : ViewModelBase
{
    public string EntLumpName { get; private set; } = "efewf";

    private ObservableCollection<EntityPropertyViewModel> _filteredProps = new();
    public ObservableCollection<EntityPropertyViewModel> FilteredProps
    {
        get { return _filteredProps; }
        set { this.RaiseAndSetIfChanged(ref _filteredProps, value); }
    }
    public List<EntityPropertyViewModel> Properties { get; set; } = new();
    public List<EntityOutputViewModel> Connections { get; } = new();

    public const string HammerId = "hammeruniqueid";
    public const string MissingClassname = "<ERROR>";
    public string Classname { get; private set; } = "";

    public string ListTargetname { get; private set; } = "";
    private string Worldname = "";

    public EntityViewModel(Utils.Entity entity)
    {
        EntLumpName = entity.EntLumpName;

        List<EntityPropertyViewModel> _props = new();
        foreach (var property in entity.Properties)
        {
            string k = property.Key.ToLower();
            var value = property.Value;
            string _value = "";

            if (value == null)
            {
                _value = "null";
            }
            else if (value is KVObject kvArray)
            {
                _value = $"[{string.Join(", ", kvArray.Select(p => p.Value.ToString()).ToArray())}]";
            }
            else
            {
                _value = value.ToString()!;
            }

            switch(k)
            {
                case "targetname":
                    if (value == null || _value == "")
                        break;
                    ListTargetname = _value;
                    break;
                case "classname":
                    Classname = _value;
                    break;
                case "worldname":
                    Worldname = _value;
                    break;
            }

            _props.Add(new EntityPropertyViewModel(property.Key, _value));
        }

        if (Classname == "")
            Classname = MissingClassname;

        if (ListTargetname == "" && Worldname.Length > 0)
            ListTargetname = Worldname;

        SortProperties(_props);

        if (entity.Connections != null)
        {
            foreach (var connection in entity.Connections)
            {
                string output = connection.GetProperty<string>("m_outputName");
                string target = connection.GetProperty<string>("m_targetName");
                string input = connection.GetProperty<string>("m_inputName");
                string param = connection.GetProperty<string>("m_overrideParam", "");

                if (param == "(null)")
                {
                    param = "";
                }

                float delay = connection.GetFloatProperty("m_flDelay");
                int timesToFire = connection.GetInt32Property("m_nTimesToFire");

                Connections.Add(new EntityOutputViewModel(output, target, input, param, delay, timesToFire));
            }
        }
    }

    void SortProperties(List<EntityPropertyViewModel> _props)
    {
        foreach (string i in EntityImportantKeys.ImportantKeys)
        {
            for(int j = _props.Count-1; j >= 0;j--)
            {
                if (_props[j].Key == i)
                {
                    Properties.Add(_props[j]);
                    _props.RemoveAt(j);
                }
            }
        }
        foreach (var p in _props)
        {
            Properties.Add(p);
        }
    }

    public void FilterProperties(string filter)
    {
        FilteredProps.Clear();

        if(filter == "")
        {
            foreach(var prop in Properties)
            {
                prop.Matched = false;
                FilteredProps.Add(prop);
            }

            return;
        }

        int matched = 0;
        foreach (var prop in Properties)
        {
            if(prop.Key.ToLower().Contains(filter) || prop.Value.ToLower().Contains(filter))
            {
                prop.Matched = true;
                FilteredProps.Insert(matched, prop);
                matched++;
            }
            else
            {
                prop.Matched = false;
                FilteredProps.Add(prop);
            }
        }
    }

    public bool BasicSearch(string text)
    {
        if (text == "")
            return true;

        foreach (var p in Properties)
        {
            if (p.Key.ToLower().Contains(text) || p.Value.ToLower().Contains(text))
                return true;
        }

        foreach (var c in Connections)
        {
            if (c.BasicSearch(text))
                return true;
        }

        return false;
    }

    public bool SearchProperties(string key, string value)
    {
        if (key == "" && value == "")
            return true;

        foreach (var p in Properties)
        {
            if(p.Key.ToLower().Contains(key))
            {
                if(p.Value.ToLower().Contains(value)) 
                    return true;
            }
        }

        return false;
    }

    public bool SearchConnections(string text)
    {
        if (text == "")
            return true;

        foreach (var c in Connections)
        {
            if (c.BasicSearch(text))
                return true;
        }

        return false;
    }
}
