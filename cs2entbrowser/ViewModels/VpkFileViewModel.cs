using cs2entbrowser.Utils;
using cs2entbrowser.ViewModels.Entity;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.ViewModels;

public class VpkFileViewModel : ViewModelBase
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public List<EntityLumpViewModel> EntityLumps { get; private set; } = new();
    private bool? _enabled = false;
    public bool? Enabled
    {
        get => _enabled;
        set => this.RaiseAndSetIfChanged(ref _enabled, value);
    }

    private ItemUpdating Updating = ItemUpdating.NotUpdating;

    private bool _updated = false;
    public bool Updated
    {
        get => _updated;
        set => this.RaiseAndSetIfChanged(ref _updated, value);
    }

    public VpkFileViewModel(VpkFile vpk)
    {
        Id = vpk.Id;
        Name = vpk.Name;

        this.WhenAnyValue(x => x.Enabled)
            .Subscribe(_ => LumpChanged());

        foreach (var lump in vpk.EntityLumps)
        {
            EntityLumps.Add(new EntityLumpViewModel(lump));
            EntityLumps[EntityLumps.Count - 1].WhenAnyValue(x => x.Enabled)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => UpdateState());
        }
    }

    public void LumpChanged()
    {
        if(Updating == ItemUpdating.NotUpdating)
        {
            Updating = ItemUpdating.VpkUpdating;
            
            // Set child lumps to either all false, or all true
            if(Enabled == true)
            {
                foreach (var lump in EntityLumps)
                {
                    lump.Enabled = true;
                }
                //Enabled = true;
            }
            else
            {
                foreach (var lump in EntityLumps)
                {
                    lump.Enabled = false;
                }
                Enabled = false;
            }
            Updating = ItemUpdating.NotUpdating;
            Updated = !Updated;
        }
    }

    public void UpdateState()
    {
        if (Updating == ItemUpdating.NotUpdating)
        { 
            Updating = ItemUpdating.LumpUpdating; 

            int disabled = 0;
            int enabled = 0;

            foreach (var lump in EntityLumps)
            {
                if (lump.Enabled)
                    enabled++;
                else
                    disabled++;
            }

            if (enabled > 0)
            {
                if (disabled > 0)
                    Enabled = null;
                else
                    Enabled = true;
            }
            else
                Enabled = false;

            Updating = ItemUpdating.NotUpdating;
            Updated = !Updated;
        }
    }
}
