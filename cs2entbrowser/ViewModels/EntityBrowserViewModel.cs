using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using cs2entbrowser.Services;
using cs2entbrowser.Utils;
using cs2entbrowser.ViewModels.Entity;
using cs2entbrowser.Views;
using ReactiveUI;

namespace cs2entbrowser.ViewModels;

class EntityBrowserViewModel : ViewModelBase
{
    public EntityBrowserView View { get; private set; }

    private string _loadedVpkTitle = "";
    public string LoadedVpkTitle
    {
        get => _loadedVpkTitle;
        set => this.RaiseAndSetIfChanged(ref _loadedVpkTitle, value);
    }

    private bool _isAdvancedSearch = false;
    public bool IsAdvancedSearch
    {
        get => _isAdvancedSearch;
        set => this.RaiseAndSetIfChanged(ref _isAdvancedSearch, value);
    }

    private string _basicSearchText = "";
    public string BasicSearchText
    {
        get => _basicSearchText;
        set => this.RaiseAndSetIfChanged(ref _basicSearchText, value);
    }

    private string _advancedSearchKeyText = "";
    public string AdvancedSearchKeyText
    {
        get => _advancedSearchKeyText;
        set => this.RaiseAndSetIfChanged(ref _advancedSearchKeyText, value);
    }

    private string _advancedSearchValueText = "";
    public string AdvancedSearchValueText
    {
        get => _advancedSearchValueText;
        set => this.RaiseAndSetIfChanged(ref _advancedSearchValueText, value);
    }

    private string _advancedSearchOutputText = "";
    public string AdvancedSearchOutputText
    {
        get => _advancedSearchOutputText;
        set => this.RaiseAndSetIfChanged(ref _advancedSearchOutputText, value);
    }

    private string _propertySearchText = "";
    public string PropertySearchText
    {
        get => _propertySearchText;
        set => this.RaiseAndSetIfChanged(ref _propertySearchText, value);
    }

    private ObservableCollection<VpkFileViewModel> _vpkFiles = new();
    public ObservableCollection<VpkFileViewModel> VpkFiles
    {
        get { return _vpkFiles; }
        set { this.RaiseAndSetIfChanged(ref _vpkFiles, value); }
    }

    private ObservableCollection<EntityViewModel> _filteredItems = new();
    public ObservableCollection<EntityViewModel> FilteredItems
    {
        get { return _filteredItems; }
        set { this.RaiseAndSetIfChanged(ref _filteredItems, value); }
    }

    private EntityViewModel _selectedItem;
    public EntityViewModel _SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }

    private EntityViewModel _safeSelectedItem;
    public EntityViewModel SelectedItem
    {
        get => _safeSelectedItem;
        set => this.RaiseAndSetIfChanged(ref _safeSelectedItem, value);
    }

    private bool _isLoaded = false;
    public bool IsLoaded
    {
        get => _isLoaded;
        set => this.RaiseAndSetIfChanged(ref _isLoaded, value);
    }
    public EntityBrowserViewModel() { }
    public EntityBrowserViewModel(EntityBrowserView view)
    {
        View = view;

        this.WhenAnyValue(x => x._SelectedItem)
            .Subscribe(_ => SelectionChanged());

        this.WhenAnyValue(x => x.IsAdvancedSearch)
            .Subscribe(_ => AdvancedSearchChanged());

        this.WhenAnyValue(x => x.BasicSearchText)
            .Subscribe(_ => BasicSearch());

        this.WhenAnyValue(x => x.AdvancedSearchKeyText)
            .Subscribe(_ => AdvancedSearch());

        this.WhenAnyValue(x => x.AdvancedSearchValueText)
            .Subscribe(_ => AdvancedSearch());

        this.WhenAnyValue(x => x.AdvancedSearchOutputText)
            .Subscribe(_ => AdvancedSearch());

        this.WhenAnyValue(x => x.PropertySearchText)
            .Subscribe(_ => PropertySearch());

        SettingsService.Instance.WhenAnyValue(x => x.Loaded)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SettingsLoadedStateChanged());

        VpkService.Instance.WhenAnyValue(x => x.LoadedVpk)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LoadedVpkChanged());

        VpkService.Instance.WhenAnyValue(x => x.LoadedTitle)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LoadedTitleChanged());

        VpkService.Instance.WhenAnyValue(x => x.State)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LoadedStateChanged());
    }

    public void Unload()
    {
        VpkFiles.Clear();
        FilteredItems.Clear();
        IsLoaded = false;
        BasicSearchText = "";
        PropertySearchText = "";
    }

    void SettingsLoadedStateChanged()
    {
        if (SettingsService.Instance.Loaded)
        {
            IsAdvancedSearch = SettingsService.Instance.IsUsingDetailedSearch;
        }
    }

    public void SelectionChanged()
    {
        if (_SelectedItem == null)
            return;
        Debug.WriteLine("Now selected: " + _SelectedItem.Classname);
        SelectedItem = _SelectedItem;
        SelectedItem.FilterProperties(PropertySearchText.Trim().ToLower());
    }

    public void LumpsChanged()
    {
        if (IsAdvancedSearch)
            AdvancedSearch();
        else
            BasicSearch();
    }

    public void AdvancedSearchChanged()
    {
        SettingsService.Instance.IsUsingDetailedSearch = IsAdvancedSearch;
        SettingsService.Instance.WriteSettings();

        if(IsAdvancedSearch)
            AdvancedSearch();
        else
            BasicSearch();
    }

    public void BasicSearch()
    {
        if (!IsLoaded)
            return;

        if (IsAdvancedSearch)
            return;

        Debug.WriteLine("Basic Searching for: " + BasicSearchText);
        FilteredItems.Clear();
        
        string search = BasicSearchText.ToLower();
        foreach (var file in VpkFiles)
        {
            foreach (var lump in file.EntityLumps)
            {
                if (!lump.Enabled)
                    continue;

                foreach (var ent in lump.Entities)
                {
                    if (ent.BasicSearch(search))
                    {
                        FilteredItems.Add(ent);
                        if(ent == SelectedItem)
                        {
                            _SelectedItem = SelectedItem;
                            SelectedIndex = FilteredItems.Count - 1;
                        }
                    }
                }
            }
        }
    }

    public void AdvancedSearch()
    {
        if (!IsLoaded)
            return;

        if (!IsAdvancedSearch)
            return;

        Debug.WriteLine("Advanced Searching for: ");
        Debug.WriteLine(AdvancedSearchKeyText + "   :   " + AdvancedSearchValueText + "   :   " + AdvancedSearchOutputText);
        FilteredItems.Clear();

        string keySearch = AdvancedSearchKeyText.ToLower();
        string valueSearch = AdvancedSearchValueText.ToLower();
        string outputSearch = AdvancedSearchOutputText.ToLower();
        foreach (var file in VpkFiles)
        {
            foreach (var lump in file.EntityLumps)
            {
                if (!lump.Enabled)
                    continue;

                foreach (var ent in lump.Entities)
                {
                    if (ent.SearchProperties(keySearch, valueSearch) && ent.SearchConnections(outputSearch))
                    {
                        FilteredItems.Add(ent);
                        if (ent == SelectedItem)
                        {
                            _SelectedItem = SelectedItem;
                            SelectedIndex = FilteredItems.Count - 1;
                        }
                    }
                }
            }
        }
    }

    public void PropertySearch()
    {
        if (!IsLoaded)
            return;

        Debug.WriteLine("Searching properties for: " + PropertySearchText);

        if(SelectedItem != null)
            SelectedItem.FilterProperties(PropertySearchText.Trim().ToLower());
    }
    public void LoadedVpkChanged()
    {
        Debug.WriteLine("EBVM:: Now loaded: " + VpkService.Instance.LoadedVpk);
    }

    public void LoadedTitleChanged()
    {
        Debug.WriteLine("new loaded title: "+ VpkService.Instance.LoadedTitle);
        LoadedVpkTitle = VpkService.Instance.LoadedTitle;
    }

    public void LoadedStateChanged()
    {
        Debug.WriteLine("STATE CHANGED TO: " + (int)(VpkService.Instance.State));
        if(VpkService.Instance.State == LoadState.Unloaded)
        {
            Unload();
            return;
        }

        if(VpkService.Instance.State == LoadState.Loaded && VpkService.Instance.PreviousState == LoadState.Loading)
        {
            PopulateLumps();
            ShowAllEntities();
            IsLoaded = true;
        }
    }

    private void PopulateLumps()
    {
        //Add vpk names and entity lump names to the checkbox exanders
        foreach (var file in VpkService.Instance.VpkFiles)
        {
            VpkFiles.Add(file);
            Debug.WriteLine("!!!!VPKFILE.NAME: " + file.Name);
            VpkFiles[VpkFiles.Count - 1].WhenAnyValue(x => x.Updated)
                .Subscribe(_ => LumpsChanged());
        }
        
    }

    private void ShowAllEntities()
    {
        Debug.WriteLine("there are " + VpkFiles.Count + " vpkfiles");
        foreach (var file in VpkFiles)
        {
            Debug.WriteLine("there are " + file.EntityLumps.Count + " entitylumps");
            foreach (var lump in file.EntityLumps)
            {
                Debug.WriteLine("there are " + lump.Entities.Count + " entities");
                foreach (var ent in lump.Entities)
                {
                    FilteredItems.Add(ent);
                }
            }
        }
        Debug.WriteLine("added " + FilteredItems.Count.ToString() + " to list");
    }

    public void UpdateVpkCheckbox()
    {
        foreach (var vpk in VpkFiles)
        {
            vpk.UpdateState();
        }
    }
}
