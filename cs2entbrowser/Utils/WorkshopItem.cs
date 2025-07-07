using cs2entbrowser.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Avalonia.Controls;
using cs2entbrowser.Services;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using MsBox.Avalonia;
using Avalonia.Controls.Shapes;

namespace cs2entbrowser.Utils;

class WorkshopItem : ViewModelBase
{
    private string url = @"https://steamcommunity.com/sharedfiles/filedetails/?id=";
    private string Folder = "";
    public bool HasDuplicate { get; set; } = false;

    public string Id { get; set; } = "";

    public string Title { get; set; } = "";

    private string bytesSizeText = "";
    public string BytesSizeText
    {
        get => bytesSizeText;
        set => this.RaiseAndSetIfChanged(ref bytesSizeText, value);
    }

    private string sizeText = "";
    public string SizeText
    {
        get => sizeText;
        set => this.RaiseAndSetIfChanged(ref sizeText, value);
    }
    public WorkshopItem(string _folder, string name)
    {
        // Need backslash for explorer to open properly
        Folder = _folder.Replace('/','\\'); 
        Id = System.IO.Path.GetFileName(Folder);
        Title = name;

        url += Id;

        GetVpkSize();

        //Debug.WriteLine("id:"+Id+"  Name:"+Title+"  Folder:"+Folder);
    }

    async void GetVpkSize()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Folder);
        long size = 0;
        size += await DirectorySizeCalculator.GetDirectorySizeAsync(directoryInfo);

        double gigabytes = 0.0;
        double megabytes = (size / 1024.0) / 1024.0;
        if (megabytes > 1024.0)
            gigabytes = megabytes / 1024.0;

        BytesSizeText = size.ToString("N0");

        if (gigabytes != 0.0)
            SizeText = "(" + gigabytes.ToString("F2") + "GB)";
        else
            SizeText = "(" + megabytes.ToString("F2") + "MB)";
    }

    public void FolderClick()
    {
        Debug.WriteLine("Clicked " + Title + " Folder");
        Process.Start("explorer.exe", Folder);
    }
    public void SteamClick()
    {
        Debug.WriteLine("Clicked " + Title + " Steam");
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    public void EntBrowserClick()
    {
        Debug.WriteLine("Clicked " + Title + " EntBrowser");

        string vpk = Folder + "\\" + Id + "_dir.vpk";
        if(File.Exists(vpk))
        {
            VpkService.Instance.RequestLoad(vpk, Title);
        }
        else
        {
            vpk = Folder + "\\" + Id + ".vpk";
            if(File.Exists(vpk))
            {
                VpkService.Instance.RequestLoad(vpk, Title);
            }
            else
            {
                ShowVpkFileError();
            }
        }
    }

    private async void ShowVpkFileError()
    {
        // Previewer HATES messagebox
        if (!Design.IsDesignMode)
        {
            string message = "Failed to find a valid .vpk file for this addon.";
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
