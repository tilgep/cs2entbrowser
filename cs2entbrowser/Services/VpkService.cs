using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using cs2entbrowser.ViewModels.Entity;
using cs2entbrowser.Utils;
using cs2entbrowser.Utils.Parser;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using MsBox.Avalonia;
using cs2entbrowser.ViewModels;
using System.Runtime.CompilerServices;

namespace cs2entbrowser.Services;

public enum LoadState
{
    Unloaded,
    Unloading,
    Loading,
    Loaded,
}
public enum Page
{
    WorkshopBrowser,
    EntityBrowser,
}

public enum SearchType
{
    Search,
    Jump,
}

public enum SearchTarget
{
    Key,
    Value,
    Output,
}

public sealed class VpkService : ReactiveObject
{
    public static VpkService Instance { get; } = new();

    public int LumpId { get; set; } = 0;

    private string _loadedVpk = "";
    public string RequestedPath
    {
        get => _loadedVpk;
        set => this.RaiseAndSetIfChanged(ref _loadedVpk, value);
    }

    public string RequestedTitle = "";

    public SearchTarget searchTarget { get; private set; }
    public SearchType searchType { get; private set; }
    private string _requestedSearchText = "";
    public string RequestedSearch
    {
        get => _requestedSearchText;
        set => this.RaiseAndSetIfChanged(ref _requestedSearchText, value);
    }

    public LoadState PreviousState = LoadState.Unloaded;

    private LoadState _state = LoadState.Unloaded;
    public LoadState State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }

    public List<string> OpenPaths = new();

    private void SetState(LoadState newState)
    {
        PreviousState = State;
        State = newState;
    }

    public void RequestLoad(string path, string title)
    {
        RequestedTitle = title;
        RequestedPath = path;
    }

    public void RequestSearch(SearchType type, SearchTarget target, string text)
    {
        Debug.WriteLine("target is " + (int)target);
        searchType = type;
        searchTarget = target;
        RequestedSearch = text;
    }

    public void SearchFinished()
    {
        RequestedSearch = string.Empty;
    }

    public LoadedVpk? OpenVpk(string _path, string _title = "")
    {
        if (_path == "")
            return null;

        // Check if already loaded
        for(int i = 0; i < OpenPaths.Count; i++)
        {
            if (OpenPaths[i] == _path)
                return null;
        }


        Debug.WriteLine("Service:: Notifying new VPK is being opened: " + _path);
        //LoadedVpk = _path;
        //LoadedTitle = _title;

        List<VpkFile>? _vpks = EntityLumpParser.ParseFromVpk(_path);
        if(_vpks == null)
        {
            //UnloadVpk();
            ShowVpkFileError();
            return null;
        }

        SettingsService.Instance.AddRecentFile(_path, _title);

        LoadedVpk vpk = new LoadedVpk(_title, _path);
        List<VpkFileViewModel> VpkFiles = _vpks.Select(ParseVpkViewModel).ToList();

        for(int i = 0;i < VpkFiles.Count;i++)
            vpk.VpkFiles.Add(VpkFiles[i]);

        Debug.WriteLine("VpkService parsed: " + VpkFiles.Count.ToString() + " vpkfiles");
        //SetState(LoadState.Loaded);
        return vpk;
    }

    private static VpkFileViewModel ParseVpkViewModel(VpkFile vpk)
    {
        return new VpkFileViewModel(vpk);
    }

    public void UnloadVpk()
    {
        SetState(LoadState.Unloading);
        //LoadedTitle = "";
        //LoadedVpk = "";
        //VpkFiles.Clear();
        LumpId = 0;
        SetState(LoadState.Unloaded);
    }

    private async void ShowVpkFileError()
    {
        // Previewer HATES messagebox
        if (!Design.IsDesignMode)
        {
            string message = "Failed to parse entities from this VPK.";
            var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ButtonDefinitions = new List<ButtonDefinition>
                    {
                        new ButtonDefinition { Name = "Ok" }
                    },
                ContentTitle = "Error",
                ContentMessage = message,
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                MaxWidth = 500,
                MaxHeight = 800,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInCenter = true,
                Topmost = true,
            });
            var result = await box.ShowAsync();
        }
    }
}
