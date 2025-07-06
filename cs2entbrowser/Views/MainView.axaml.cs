using Avalonia.Controls;
using Avalonia.Platform.Storage;
using cs2entbrowser.Services;
using cs2entbrowser.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace cs2entbrowser.Views;

public partial class MainView : UserControl
{
    private MainViewModel _vm;
    public MainView()
    {
        InitializeComponent();

        _vm = new MainViewModel(this);
        DataContext = _vm;
    }

    public void AddRecentFiles()
    {
        List<MenuItem> recentFiles = new List<MenuItem>();

        if (SettingsService.Instance.RecentFiles.Count == 0)
        {
            RecentFiles.Items.Add(new MenuItem
            {
                Header = "_No Recent Files",
                IsEnabled = false
            });
        }
        else
        {
            for(int i = 0; i < SettingsService.Instance.RecentFiles.Count; i++)
            {
                RecentFile rf = SettingsService.Instance.RecentFiles[i];

                const int max = 50;
                string header = rf.Path.Length > max ? "..." + rf.Path[^max..] : rf.Path;
                header = "_" + rf.Title + header;

                RecentFiles.Items.Add(new MenuItem
                {
                    Header = header,
                    Command = ReactiveCommand.CreateFromTask(async () => await VpkService.Instance.OpenVpk(rf.Path, rf.Title)),
                });
            }
        }
    }
}
