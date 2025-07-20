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
using Avalonia.Controls;
using cs2entbrowser.Controls;
using cs2entbrowser.Services;
using cs2entbrowser.Utils;
using cs2entbrowser.ViewModels.Entity;
using cs2entbrowser.Views;
using ReactiveUI;

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
    public EntityBrowserViewModel(EntityBrowserView view, LoadedVpk vpk)
    {
        View = view;

        if (SettingsService.Instance.Loaded)
        {
            IsAdvancedSearch = SettingsService.Instance.IsUsingDetailedSearch;
        }

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
                    if (ent.BasicSearch(search, searchText, outputChain))
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
                    if (ent.SearchProperties(keyRegex, valueRegex) && 
                        ent.SearchConnections(outputRegex, outputSearch, outputChain))
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
