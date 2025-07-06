using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using cs2entbrowser.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cs2entbrowser.Views;

public partial class WorkshopBrowserView : UserControl
{
    private WorkshopBrowserViewModel _vm;
    public WorkshopBrowserView()
    {
        InitializeComponent();

        _vm = new WorkshopBrowserViewModel(this);
        DataContext = _vm;
    }

    public async void BrowseBtn_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new FolderPickerOpenOptions
        {
            Title = "Choose workshop folder...",
            AllowMultiple = false
        };

        var toplevel = TopLevel.GetTopLevel(this);
        if (toplevel == null)
            return;

        IReadOnlyList<IStorageFolder> result = await toplevel.StorageProvider.OpenFolderPickerAsync(dialog);

        if (result.Count == 0)
            return;

        ((WorkshopBrowserViewModel)DataContext).WorkshopFolder = result[0].Path.AbsolutePath;
    }
}