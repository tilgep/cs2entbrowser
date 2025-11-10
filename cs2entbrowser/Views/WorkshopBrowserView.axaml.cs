using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using cs2entbrowser.Utils;
using cs2entbrowser.ViewModels;
using DynamicData.Binding;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace cs2entbrowser.Views;

public partial class WorkshopBrowserView : UserControl
{
    private WorkshopBrowserViewModel _vm;

    const string Ascending = "\u2B9D"; 
    const string Descending = "\u2B9F";

    public WorkshopBrowserView()
    {
        InitializeComponent();

        _vm = new WorkshopBrowserViewModel(this);
        DataContext = _vm;
    }

    WorkshopBrowserViewModel Viewmodel()
    {
        return ((WorkshopBrowserViewModel)DataContext);
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

        Viewmodel().WorkshopFolder = result[0].Path.AbsolutePath;
    }

    public void SortId(object sender, RoutedEventArgs e)
    {
        WorkshopSort currentSortType = Viewmodel().SortType;

        if (currentSortType != WorkshopSort.Id)
        {
            Viewmodel().SortType = WorkshopSort.Id;
            Viewmodel().SortDirection = SortDirection.Ascending;
        }
        else
        {
            SortDirection currentSortDirection = Viewmodel().SortDirection;
            if (currentSortDirection == SortDirection.Ascending)
                Viewmodel().SortDirection = SortDirection.Descending;
            else
                Viewmodel().SortDirection = SortDirection.Ascending;
        }

        Viewmodel().UpdateSort();
        sortIdButton.Content = "Workshop ID " + (Viewmodel().SortDirection == SortDirection.Ascending ? Ascending : Descending);
        sortNameButton.Content = "Addon Name";
        sortSizeButton.Content = "VPK Size (Bytes)";
    }

    public void SortName(object sender, RoutedEventArgs e)
    {
        WorkshopSort currentSortType = Viewmodel().SortType;

        if (currentSortType != WorkshopSort.Name)
        {
            Viewmodel().SortType = WorkshopSort.Name;
            Viewmodel().SortDirection = SortDirection.Ascending;
        }
        else
        {
            SortDirection currentSortDirection = Viewmodel().SortDirection;
            if (currentSortDirection == SortDirection.Ascending)
                Viewmodel().SortDirection = SortDirection.Descending;
            else
                Viewmodel().SortDirection = SortDirection.Ascending;
        }

        Viewmodel().UpdateSort();
        sortIdButton.Content = "Workshop ID";
        sortNameButton.Content = "Addon Name " + (Viewmodel().SortDirection == SortDirection.Ascending ? Ascending : Descending);
        sortSizeButton.Content = "VPK Size (Bytes)";
    }

    public void SortSize(object sender, RoutedEventArgs e)
    {
        WorkshopSort currentSortType = Viewmodel().SortType;

        if (currentSortType != WorkshopSort.Size)
        {
            Viewmodel().SortType = WorkshopSort.Size;
            Viewmodel().SortDirection = SortDirection.Ascending;
        }
        else
        {
            SortDirection currentSortDirection = Viewmodel().SortDirection;
            if (currentSortDirection == SortDirection.Ascending)
                Viewmodel().SortDirection = SortDirection.Descending;
            else
                Viewmodel().SortDirection = SortDirection.Ascending;
        }

        Viewmodel().UpdateSort();
        sortIdButton.Content = "Workshop ID";
        sortNameButton.Content = "Addon Name";
        sortSizeButton.Content = "VPK Size (Bytes) " + (Viewmodel().SortDirection == SortDirection.Ascending ? Ascending : Descending);
    }
}