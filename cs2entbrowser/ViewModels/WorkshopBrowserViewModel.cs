using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using cs2entbrowser.Services;
using cs2entbrowser.Utils;
using cs2entbrowser.ViewModels.Entity;
using cs2entbrowser.Views;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace cs2entbrowser.ViewModels;

class WorkshopBrowserViewModel : ViewModelBase
{
    public WorkshopBrowserView View { get; private set; }
    public bool IsFiltered { get; set; } = false;

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    private string workshopFolder = "";
    public string WorkshopFolder
    {
        get => workshopFolder;
        set
        {
            this.RaiseAndSetIfChanged(ref workshopFolder, value);
            FolderChanged();
        }
    }

    // All workshop items
    public List<WorkshopItem> _items { get; private set; } = new();

    // Workshop items that include the search text
    private ObservableCollection<WorkshopItem> _filteredItems = new();
    public ObservableCollection<WorkshopItem> FilteredItems
    {
        get { return _filteredItems; }
        set { this.RaiseAndSetIfChanged(ref _filteredItems, value); }
    }

    private int _itemCount;
    public int ItemCount
    {
        get => _itemCount;
        set => this.RaiseAndSetIfChanged(ref _itemCount, value);
    }

    // Needed for previewer
    public WorkshopBrowserViewModel()
    { }
    public WorkshopBrowserViewModel(WorkshopBrowserView view)
    {
        View = view;

        SettingsService.Instance.WhenAnyValue(x => x.Loaded)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SettingsLoadedStateChanged());

        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(x => Filter());
    }
    private void ClearItems()
    {
        ItemCount = 0;
        _items.Clear();
        FilteredItems.Clear();
    }

    void SettingsLoadedStateChanged()
    {
        if(SettingsService.Instance.Loaded)
        {
            WorkshopFolder = SettingsService.Instance.GetWorkshopFolder();
        }
    }

    async void FolderChanged()
    {
        if (WorkshopFolder == "")
            return;

        ClearItems();
        SearchText = "";
        
        List<WorkshopItem>? items = await WorkshopScanner.ScanWorkshopFolder(WorkshopFolder);
        if (items == null)
            return;
        
        ItemCount = items.Count;

        foreach (var item in items)
        {
            _items.Add(item);
            FilteredItems.Add(item);
        }

        //Debug.WriteLine("Workshop folder changed to: " + WorkshopFolder);
        SettingsService.Instance.SetWorkshopFolder(WorkshopFolder);
        SettingsService.Instance.WriteSettings();
    }

    public void RefreshClick()
    {
        FolderChanged();
    }

    private void Filter()
    {
        FilteredItems.Clear();
        foreach (var item in _items)
        {
            if (item.Title.ToLower().Contains(SearchText.ToLower()) || item.Id.ToLower().Contains(SearchText.ToLower()))
                FilteredItems.Add(item);
        }
    }
}
