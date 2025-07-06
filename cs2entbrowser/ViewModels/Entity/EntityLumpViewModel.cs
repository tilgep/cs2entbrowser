using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;
using cs2entbrowser.Utils.Parser.KV3;
using cs2entbrowser.Utils;
using ReactiveUI;

namespace cs2entbrowser.ViewModels.Entity;

public class EntityLumpViewModel : ViewModelBase
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string[] ChildLumps { get; private set; }
    public List<EntityViewModel> Entities { get; private set; } = new();

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set => this.RaiseAndSetIfChanged(ref _enabled, value);
    }

    public EntityLumpViewModel(EntityLump lump)
    {
        Id = lump.Id;
        Name = lump.Name;
        ChildLumps = lump.ChildLumps;
        foreach(var e in lump.Entities)
        {
            Entities.Add(new EntityViewModel(e));
        }
    }

    public void ToggleVisibilty()
    {
        Enabled = !Enabled;
    }
}
