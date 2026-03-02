using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using cs2entbrowser.Controls;
using cs2entbrowser.Services;
using cs2entbrowser.Utils;
using cs2entbrowser.ViewModels.Entity;
using cs2entbrowser.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace cs2entbrowser.ViewModels;

class EntityBrowserViewModel : ViewModelBase
{
    public EntityBrowserView View { get; private set; }

    public string Title;
    public string Path;

    private bool _isAdvancedSearch = false;
    public bool IsAdvancedSearch
    {
        get => _isAdvancedSearch;
        set => this.RaiseAndSetIfChanged(ref _isAdvancedSearch, value);
    }

    private string _classnameSearchText = "";
    public string ClassnameSearchText
    {
        get => _classnameSearchText;
        set => this.RaiseAndSetIfChanged(ref _classnameSearchText, value);
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
    public EntityViewModel SelectedEntity
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

    private bool _showRawProperties = false;
    public bool ShowRawProperties
    {
        get => _showRawProperties;
        set => this.RaiseAndSetIfChanged(ref _showRawProperties, value);
    }

    private ObservableCollection<EntityViewModel> _tabs = new();
    public ObservableCollection<EntityViewModel> SelectedEntityTabs
    {
        get { return _tabs; }
        set { this.RaiseAndSetIfChanged(ref _tabs, value); }
    }
    private int _selectedEntityIndex = 0;
    public int SelectedEntityIndex
    {
        get => _selectedEntityIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedEntityIndex, value);
    }

    private EntityViewModel _entity;
    public EntityViewModel Entity
    {
        get => _entity;
        set => this.RaiseAndSetIfChanged(ref _entity, value);
    }

    public EntityBrowserViewModel() 
    {}
    public EntityBrowserViewModel(EntityBrowserView view, LoadedVpk vpk)
    {
        View = view;

        if (SettingsService.Instance.Loaded)
        {
            IsAdvancedSearch = SettingsService.Instance.IsUsingDetailedSearch;
            ShowRawProperties = SettingsService.Instance.RawProperties;
        }

        this.WhenAnyValue(x => x._SelectedItem)
            .Subscribe(_ => SelectionChanged());

        this.WhenAnyValue(x => x.IsAdvancedSearch)
            .Subscribe(_ => AdvancedSearchChanged());

        this.WhenAnyValue(x => x.ClassnameSearchText)
            .Subscribe(_ => ClassnameSearch());

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

        this.WhenAnyValue(x => x.SelectedEntityIndex)
            .Subscribe(_ => SelectedTabChanged());

        SettingsService.Instance.WhenAnyValue(x => x.Loaded)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SettingsLoadedStateChanged());

        SettingsService.Instance.WhenAnyValue(x => x.RawProperties)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => RawPropertiesChanged());

        Title = vpk.Title;
        Path = vpk.Path;
        PopulateLumps(vpk);
        ShowAllEntities();
        IsLoaded = true;
    }

    public void Unload()
    {
        VpkFiles.Clear();
        FilteredItems.Clear();
        IsLoaded = false;
        BasicSearchText = "";
        PropertySearchText = "";
    }

    private void SelectedTabChanged()
    {
        if (SelectedEntityIndex < 0 || SelectedEntityIndex >= SelectedEntityTabs.Count)
            return;

        if (SelectedEntityTabs[SelectedEntityIndex]  != null)
        {
            Entity = SelectedEntityTabs[SelectedEntityIndex];
        }
    }

    private void PinChanged()
    {
        for (int i = SelectedEntityTabs.Count-1; i >= 0 ; i--)
        {
            if (i == SelectedEntityIndex || i < 0)
                continue;

            if (SelectedEntityTabs[i].IsPinned)
                continue;

            SelectedEntityTabs[i].disposable.Dispose();
            SelectedEntityTabs.RemoveAt(i);
        }
    }

    private void AddEntityTab(EntityViewModel entity, bool pin)
    {
        int index = SelectedEntityTabs.IndexOf(entity);
        if (index != -1)
        {
            if (pin && !entity.IsPinned)
                entity.IsPinned = true;

            SelectedEntityIndex = index;
            return;
        }

        entity.disposable = entity.WhenAnyValue(x => x.IsPinned)
            .Subscribe(_ => PinChanged());

        entity.IsPinned = pin;
        if (SelectedEntityTabs.Count == 0)
        {
            SelectedEntityTabs.Add(entity);
            SelectedEntityIndex = 0;
            return;
        }

        if (SelectedEntityTabs[SelectedEntityTabs.Count - 1].IsPinned)
        {
            SelectedEntityTabs.Add(entity);
            SelectedEntityIndex = SelectedEntityTabs.Count - 1;
        }
        else
        {
            if (!entity.IsPinned)
            {
                SelectedEntityTabs[SelectedEntityTabs.Count-1].disposable.Dispose();
                SelectedEntityTabs.RemoveAt(SelectedEntityTabs.Count - 1);
                SelectedEntityTabs.Add(entity);
                SelectedEntityIndex = SelectedEntityTabs.Count - 1;
            }
            else
            {
                int pos = SelectedEntityTabs.Count - 1;
                if (pos < 0) 
                    pos = 0;
                SelectedEntityTabs.Insert(pos, entity);
                SelectedEntityIndex = pos;
            }
        }
    }

    public void PointerPressedEntityList(object? sender, PointerPressedEventArgs args)
    {
        var point = args.GetCurrentPoint(null);

        if (point.Properties.IsLeftButtonPressed)
        {
            Debug.WriteLine("Pressed by left click");
            AddEntityTab(SelectedEntity, false);
        }
        else if(point.Properties.IsRightButtonPressed)
        {
            Debug.WriteLine("Pressed by right click");
            AddEntityTab(SelectedEntity, true);
        }
        else if(point.Properties.IsMiddleButtonPressed)
        {
            Debug.WriteLine("Pressed by middle click");
            AddEntityTab(SelectedEntity, true);
        }
    }

    void SettingsLoadedStateChanged()
    {
        if (SettingsService.Instance.Loaded)
        {
            IsAdvancedSearch = SettingsService.Instance.IsUsingDetailedSearch;
            ShowRawProperties = SettingsService.Instance.RawProperties;
        }
    }

    public void RawPropertiesChanged()
    {
        ShowRawProperties = SettingsService.Instance.RawProperties;
    }

    public void SelectionChanged()
    {
        if (_SelectedItem == null)
            return;
        Debug.WriteLine("Now selected: " + _SelectedItem.Classname);

        if (!_SelectedItem.ParsedRaw)
            _SelectedItem.ParseRawProperties();

        SelectedEntity = _SelectedItem;
        SelectedEntity.FilterProperties(PropertySearchText.Trim().ToLower());
        Entity = SelectedEntity;

        AddEntityTab(SelectedEntity, false);
    }

    public void ClassnameSearch()
    {
        if (IsAdvancedSearch)
            AdvancedSearch();
        else
            BasicSearch();
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

        string searchClass = ClassnameSearchText.ToLower();
        string searchText = BasicSearchText.ToLower();
        Regex search = ConvertToRegex(searchText);
        bool outputChain = searchText.Contains('>');
        foreach (var file in VpkFiles)
        {
            foreach (var lump in file.EntityLumps)
            {
                if (!lump.Enabled)
                    continue;

                foreach (var ent in lump.Entities)
                {
                    if (ent.BasicSearch(search, searchText, outputChain, searchClass))
                    {
                        FilteredItems.Add(ent);
                        if(ent == SelectedEntity)
                        {
                            _SelectedItem = SelectedEntity;
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

        string searchClass = ClassnameSearchText.ToLower();
        string keySearch = AdvancedSearchKeyText.ToLower();
        string valueSearch = AdvancedSearchValueText.ToLower();
        string outputSearch = AdvancedSearchOutputText.ToLower();

        Regex keyRegex = ConvertToRegex(keySearch);
        Regex valueRegex = ConvertToRegex(valueSearch);
        Regex outputRegex = ConvertToRegex(outputSearch);

        bool outputChain = outputSearch.Contains('>');

        foreach (var file in VpkFiles)
        {
            foreach (var lump in file.EntityLumps)
            {
                if (!lump.Enabled)
                    continue;

                foreach (var ent in lump.Entities)
                {
                    if (!ent.SearchClassname(searchClass))
                        continue;

                    if (ent.SearchProperties(keyRegex, valueRegex) && 
                        ent.SearchConnections(outputRegex, outputSearch, outputChain))
                    {
                        FilteredItems.Add(ent);
                        if (ent == SelectedEntity)
                        {
                            _SelectedItem = SelectedEntity;
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

        if(SelectedEntity != null)
            SelectedEntity.FilterProperties(PropertySearchText.Trim().ToLower());
    }

    public void LoadedTitleChanged()
    {
        //Debug.WriteLine("new loaded title: "+ VpkService.Instance.LoadedTitle);
        //LoadedVpkTitle = VpkService.Instance.LoadedTitle;
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
           // PopulateLumps();
            ShowAllEntities();
            IsLoaded = true;
        }
    }

    private void PopulateLumps(LoadedVpk vpk)
    {
        //Add vpk names and entity lump names to the checkbox exanders
        foreach (var file in vpk.VpkFiles)
        {
            VpkFiles.Add(file);
            VpkFiles[VpkFiles.Count - 1].WhenAnyValue(x => x.Updated)
                .Subscribe(_ => LumpsChanged());
        }
    }

    private void ShowAllEntities()
    {
        foreach (var file in VpkFiles)
        {
            foreach (var lump in file.EntityLumps)
            {
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

    public void JumpToCommand(string target)
    {
        Debug.WriteLine("Jumping to: " + target);
        Regex search = ConvertToRegex(target);
        Debug.WriteLine("Regex: "+search.ToString());
        foreach (var file in VpkFiles)
        {
            foreach (var lump in file.EntityLumps)
            {
                foreach (var ent in lump.Entities)
                {
                    if (ent.SearchPropertiesExact("targetname", search))
                    {
                        _SelectedItem = ent;
                        return;
                    }
                }
            }
        }
    }

    public void SearchForCommand(string target, SearchTarget searchTarget)
    {
        Debug.WriteLine("Searching for: " + target);
        if (IsAdvancedSearch)
        {
            switch(searchTarget)
            {
                case SearchTarget.Key:
                    AdvancedSearchKeyText = target; 
                    break;
                case SearchTarget.Value:
                    AdvancedSearchValueText = target; 
                    break;
                case SearchTarget.Output:
                    AdvancedSearchOutputText = target; 
                    break;
            }
        }
        else
        {
            BasicSearchText = target;
        }
    }

    public static Regex ConvertToRegex(string text)
    {
        string escaped = Regex.Escape(text);
        string pattern = escaped.Replace(@"\*", ".*");

        // Add wildcards to start and end if not already present
        if (!pattern.StartsWith(".*"))
        {
            pattern = ".*" + pattern;
        }
        if (!pattern.EndsWith(".*"))
        {
            pattern = pattern + ".*";
        }

        return new Regex(pattern, RegexOptions.IgnoreCase);
    }
}
