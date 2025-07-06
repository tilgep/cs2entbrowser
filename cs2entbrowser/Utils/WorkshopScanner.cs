using Avalonia.Controls;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace cs2entbrowser.Utils;

class WorkshopScanner
{
    public static async Task<List<WorkshopItem>?> ScanWorkshopFolder(string folder, bool sorted = true)
    {
        Debug.WriteLine("Checking folder: " + folder);
        bool folderExists = await CheckFolder(folder, false);
        if (!folderExists)
            return null;

        string cs2folder = folder;
        if (!folder.EndsWith('/'))
            cs2folder += "/";
        
        cs2folder += "content/730/";

        Debug.WriteLine("Checking cs2 folder: " + cs2folder);
        folderExists = await CheckFolder(cs2folder, true);
        if (!folderExists)
            return null;
        
        string[] dirs = Directory.GetDirectories(cs2folder);
        if (dirs.Length == 0)
            return null;

        List<WorkshopItem> items = new();

        foreach (string dir in dirs)
        {
            string id = Path.GetFileName(dir);
            string? title = GetAddonTitle(dir);
            if (title == null)
                title = "<ERROR>";
            //Debug.WriteLine("id:" + id + "   title:" + title + "  dir:"+dir);
            WorkshopItem wsItem = new WorkshopItem(dir, title);

            //Sort alphabetically
            int place = -1;
            for (int i = 0; i < items.Count; i++)
            {
                WorkshopItem other = items[i];
                int compare = string.Compare(title, other.Title, StringComparison.OrdinalIgnoreCase);
                if (compare < 0)
                {
                    place = i;
                    break;
                }
                else if (compare == 0)
                {
                    wsItem.HasDuplicate = true;
                    other.HasDuplicate = true;

                    // Sort dupes by id
                    if (string.Compare(wsItem.Id, other.Id) < 0)
                    {
                        place = i;
                        break;
                    }
                }
            }

            if (place == -1)
                items.Add(wsItem);
            else
                items.Insert(place, wsItem);
        }

        return items;
    }

    private static async Task<bool> CheckFolder(string folder, bool cs2folder)
    {
        if (!Directory.Exists(folder))
        {
            // Previewer HATES messagebox
            if(!Design.IsDesignMode)
            {
                string message = cs2folder ? "No CS2 content is in this workshop folder." :"Failed to find the specified workshop folder.";
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
            return false;
        }

        return true;
    }

    public static string? GetAddonTitle(string addonFolder)
    {
        if (!Directory.Exists(addonFolder))
            return null;

        string dataFile = addonFolder += "/publish_data.txt";
        if (!File.Exists(dataFile))
            return null;

        using (StreamReader sr = new StreamReader(dataFile))
        {
            string? line = sr.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                if (line.Contains("\"title\""))
                {
                    line = line.Replace("\"title\"", "");
                    line = line.Replace("\"", "");
                    line = line.Trim();
                    return line;
                }

                line = sr.ReadLine();
            }
        }

        return null;
    }
}
