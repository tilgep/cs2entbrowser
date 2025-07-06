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

public sealed class VpkService : ReactiveObject
{
    public static VpkService Instance { get; } = new();

    public int LumpId { get; set; } = 0;

    private string _loadedVpk = "";
    public string LoadedVpk
    {
        get => _loadedVpk;
        set => this.RaiseAndSetIfChanged(ref _loadedVpk, value);
    }

    private string _loadedTitle = "";
    public string LoadedTitle
    {
        get => _loadedTitle;
        set => this.RaiseAndSetIfChanged(ref _loadedTitle, value);
    }

    public LoadState PreviousState = LoadState.Unloaded;

    private LoadState _state = LoadState.Unloaded;
    public LoadState State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }

    public List<VpkFileViewModel> VpkFiles { get; private set; } = new();

    private void SetState(LoadState newState)
    {
        PreviousState = State;
        State = newState;
    }
    public async Task<bool> OpenVpk(string _path, string _title = "")
    {
        if (State == LoadState.Loading)
            return false;
        if(State == LoadState.Loaded)
        {
            UnloadVpk();
        }

        SetState(LoadState.Loading);

        Debug.WriteLine("Service:: Notifying new VPK is being opened: " + _path);
        LoadedVpk = _path;
        LoadedTitle = _title;

        List<VpkFile>? _vpks = EntityLumpParser.ParseFromVpk(_path);
        if(_vpks == null)
        {
            UnloadVpk();
            ShowVpkFileError();
            return false;
        }

        SettingsService.Instance.AddRecentFile(LoadedVpk, LoadedTitle);

        VpkFiles = _vpks.Select(ParseVpkViewModel).ToList();
        Debug.WriteLine("VpkService parsed: " + VpkFiles.Count.ToString() + " vpkfiles");
        SetState(LoadState.Loaded);
        return true;
    }

    private static VpkFileViewModel ParseVpkViewModel(VpkFile vpk)
    {
        return new VpkFileViewModel(vpk);
    }

    public void UnloadVpk()
    {
        SetState(LoadState.Unloading);
        LoadedTitle = "";
        LoadedVpk = "";
        VpkFiles.Clear();
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
