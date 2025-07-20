using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform.Storage;
using cs2entbrowser.Services;
using cs2entbrowser.Utils;
using cs2entbrowser.Utils.Parser;
using cs2entbrowser.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace cs2entbrowser.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainView View { get; private set; }
    public static VpkService PageService => VpkService.Instance;

    private int _activePageIndex = 0;
    public int ActivePageIndex
    {
        get => _activePageIndex;
        set => this.RaiseAndSetIfChanged(ref _activePageIndex, value);
    }

    private const string ENTITY_BROWSER = "_Entity Browser";
    private string _entityBrowserHeader = ENTITY_BROWSER;
    public string EntityBrowserHeader
    {
        get => _entityBrowserHeader;
        set => this.RaiseAndSetIfChanged(ref _entityBrowserHeader, value);
    }

    private ObservableCollection<MenuItem> _recentFiles = new();
    public ObservableCollection<MenuItem> RecentFiles
    {
        get { return _recentFiles; }
        set { this.RaiseAndSetIfChanged(ref _recentFiles, value); }
    }

    private ObservableCollection<TabItem> _tabs = new();
    public ObservableCollection<TabItem> Tabs
    {
        get { return _tabs; }
        set { this.RaiseAndSetIfChanged(ref _tabs, value); }
    }

    public MainViewModel() { }
    public MainViewModel(MainView view)
    {
        View = view;

        SettingsService.Instance.LoadSettings();

        SettingsService.Instance.WhenAnyValue(x => x.Loaded)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SettingsLoadedStateChanged());

        VpkService.Instance.WhenAnyValue(x => x.RequestedPath)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => VpkRequested());

        VpkService.Instance.WhenAnyValue(x => x.RequestedSearch)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SearchRequested());

        AddInitialTabs();
    }

    private void AddInitialTabs()
    {
        Tabs.Clear();
        Tabs.Add(new TabItem
        {
            Header = "Workshop Browser",
            Content = new WorkshopBrowserView(),
        });
    }

    private void AddTab(LoadedVpk vpk)
    {
        TabItem tab = new TabItem
        {
            Header = "_" + vpk.Title,
            Tag = vpk.Path,
            Content = new EntityBrowserView(vpk),
            ContextMenu = new ContextMenu()
        };

        tab.ContextMenu.ItemsSource = new[]
        {
            new MenuItem
            {
                Header = "Close",
                Command = ReactiveCommand.Create(() => CloseTab(tab))
            }
        };

        Tabs.Add(tab);

        ActivePageIndex = Tabs.Count - 1;

        AddRecentFiles();
    }

    public void CloseTab(TabItem tab)
    {
        VpkService.Instance.OpenPaths.Remove(((EntityBrowserView)tab.Content).GetPath());
        Tabs.Remove(tab);
    }

    public async Task OpenFileCommand()
    {
        if(VpkService.Instance.State == LoadState.Loading)
        {
            return;
        }

        var dialog = new FilePickerOpenOptions
        {
            Title = "Choose VPK to open...",
            AllowMultiple = false,
            FileTypeFilter = GetVpkFileFilter(),
        };

        var toplevel = TopLevel.GetTopLevel(View);
        IReadOnlyList<IStorageFile> result = await toplevel!.StorageProvider.OpenFilePickerAsync(dialog);

        if (result.Count == 0)
            return;

        if (!result[0].Path.IsFile || result[0].Path.AbsolutePath is null)
            return;

        string path = result[0].Path.ToString()[8..];

        // Try to find a nearby publish_data.txt for title, otherwise use vpk name
        path = path.Replace('\\', '/');
        string pathDir = path.Substring(0, path.LastIndexOf('/'));
        string? title = WorkshopScanner.GetAddonTitle(pathDir);
        if (title == null)
            title = path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.') - (path.LastIndexOf('/') + 1));
        
        LoadedVpk? vpk = VpkService.Instance.OpenVpk(path, title);
        if(vpk != null)
            AddTab(vpk);
    }

    public void CloseCommand()
    {
        if (ActivePageIndex > 0)
        {
            CloseTab(Tabs[ActivePageIndex]);
        }
    }

    public void ExitCommand()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.Shutdown();
        }
    }

    public void OptionsCommand()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            if (mainWindow != null)
            {
                OptionsWindow optionsWindow = new OptionsWindow();
                optionsWindow.Show(mainWindow);
            }
        }
    }
    public void AboutCommand()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            if (mainWindow != null)
            {
                AboutWindow aboutWindow = new AboutWindow();
                aboutWindow.Show(mainWindow);
            }
        }
    }

    private static FilePickerFileType[] GetVpkFileFilter() =>
        [
            new("VPK Files")
            {
                Patterns = ["*.vpk"],
            }
        ];

    void SettingsLoadedStateChanged()
    {
        if (SettingsService.Instance.Loaded)
        {
            AddRecentFiles();
        }
    }

    public void AddRecentFiles()
    {
        RecentFiles.Clear();
        if (SettingsService.Instance.RecentFiles.Count == 0)
        {
            RecentFiles.Add(new MenuItem
            {
                Header = "_No Recent Files",
                IsEnabled = false
            });
        }
        else
        {
            for (int i = 0; i < SettingsService.Instance.RecentFiles.Count; i++)
            {
                RecentFile rf = SettingsService.Instance.RecentFiles[i];

                const int max = 50;
                int thismax = max - rf.Title.Length;
                string header = rf.Path.Replace('\\', '/');

                if (rf.Path.Length > thismax)
                {
                    header = header[^thismax..];
                    int slash = header.IndexOf('/');
                    if (slash != -1)
                        header = header.Substring(slash, header.Length - slash);

                    header = "..." + header;
                }

                header = "_" + rf.Title + " - " + header;

                RecentFiles.Add(new MenuItem
                {
                    Header = header,
                    Command = ReactiveCommand.Create(() => OpenRecentFile(rf.Path, rf.Title)),
                });
            }
        }
    }

    public void OpenRecentFile(string path, string title)
    {
        LoadedVpk? vpk = VpkService.Instance.OpenVpk(path, title);
        if (vpk != null)
            AddTab(vpk);
    }

    public void VpkRequested()
    {
        Debug.WriteLine("VPK Requestsed");
        OpenRecentFile(VpkService.Instance.RequestedPath, VpkService.Instance.RequestedTitle);
    }

    public void SearchRequested()
    {
        if (VpkService.Instance.RequestedSearch == string.Empty)
            return;

        if (ActivePageIndex <= 0 || ActivePageIndex >= Tabs.Count)
            return;

        if (Tabs[ActivePageIndex].Content == null)
            return;

        EntityBrowserView view = (EntityBrowserView)Tabs[ActivePageIndex].Content;
        if (view == null)
            return;

        EntityBrowserViewModel vm = (EntityBrowserViewModel)view.DataContext;
        if (vm == null) 
            return;

        switch (VpkService.Instance.searchType)
        {
            case SearchType.Search:
                vm.SearchForCommand(VpkService.Instance.RequestedSearch, VpkService.Instance.searchTarget);
                break;
            case SearchType.Jump:
                vm.JumpToCommand(VpkService.Instance.RequestedSearch);
                break;
        }

        VpkService.Instance.SearchFinished();
    }
}
