using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace cs2entbrowser.Services;

public class RecentFile
{
    public string Title { get; set; }
    public string Path { get; set; }

    public RecentFile(string title, string path)
    {
        Title = title; 
        Path = path;
    }
}

public sealed class SettingsService : ReactiveObject
{
    public static SettingsService Instance { get; } = new();
    public static string SettingsFolder = "CS2EntBrowser";
    public static string SettingsFileName = "settings.json";

    public static string WorkshopFolderProperty = "WorkshopFolder";
    public static string RecentFilesProperty = "RecentFiles";
    public static string DetailedSearchProperty = "IsUsingDetailedSearch";

    public const int MAX_RECENT_FILES = 10;

    private bool _loaded = false;
    public bool Loaded
    {
        get => _loaded;
        private set => this.RaiseAndSetIfChanged(ref _loaded, value);
    }
    public string SettingsFilePath = "";

    // Default Setting Values
    string WorkshopFolder = @"C:/Program Files (x86)/Steam/steamapps/workshop";
    public List<RecentFile> RecentFiles = new();
    public bool IsUsingDetailedSearch = false;

    public string GetWorkshopFolder()
    {
        return WorkshopFolder;
    }

    public void SetWorkshopFolder(string folder)
    {
        WorkshopFolder = folder;
    }

    public void AddRecentFile(string path, string title)
    {
        for (int i = 0; i < RecentFiles.Count; i++)
        {
            if (RecentFiles[i].Path == path)
            {
                RecentFiles.RemoveAt(i);
                break;
            }
        }

        RecentFiles.Insert(0, new RecentFile(title, path));

        if(RecentFiles.Count > MAX_RECENT_FILES)
            RecentFiles.RemoveRange(MAX_RECENT_FILES, RecentFiles.Count - MAX_RECENT_FILES);

        WriteSettings();
    }

    public void LoadSettings()
    {
        string localAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!Directory.Exists(localAppdata))
        {
            //ur fucked
            return;
        }

        string AppSettingsFolder = localAppdata + "\\" + SettingsFolder;
        if (!Directory.Exists(AppSettingsFolder))
        {
            Directory.CreateDirectory(AppSettingsFolder);
        }

        string SettingsFile = AppSettingsFolder + "\\" + SettingsFileName;
        if (!File.Exists(SettingsFile))
        {
            File.Create(SettingsFile);
        }

        SettingsFilePath = SettingsFile;
        ReadSettings();
        Loaded = true;
    }

    void ReadSettings()
    {
        try
        {
            string jsonString = File.ReadAllText(SettingsFilePath);
            Debug.WriteLine(jsonString);
            using JsonDocument doc = JsonDocument.Parse(jsonString);

            JsonElement root = doc.RootElement;

            string? workshopfolder = root.GetProperty(WorkshopFolderProperty).GetString();
            if (workshopfolder != null)
            {
                WorkshopFolder = workshopfolder;
            }

            IsUsingDetailedSearch = root.GetProperty(DetailedSearchProperty).GetBoolean();

            JsonElement recentFilesElem = root.GetProperty(RecentFilesProperty);
            for (int i = 0; i < recentFilesElem.GetArrayLength(); i++)
            {
                JsonElement recentFileObj = recentFilesElem[i];
                string? path = recentFileObj.GetProperty("path").GetString();
                string? title = recentFileObj.GetProperty("title").GetString();
                if (title != null && path != null && File.Exists(path))
                {
                    bool nope = false;
                    for (int j = 0; j < RecentFiles.Count; j++)
                    {
                        if (RecentFiles[j].Path == path)
                            nope = true;
                    }

                    if (!nope)
                        RecentFiles.Add(new RecentFile(title, path));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public void WriteSettings()
    {
        try
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();
                    writer.WriteString(WorkshopFolderProperty, WorkshopFolder);
                    writer.WriteBoolean(DetailedSearchProperty, IsUsingDetailedSearch);

                    writer.WriteStartArray(RecentFilesProperty);
                    for(int i = 0; i < RecentFiles.Count; i++)
                    {
                        if (File.Exists(RecentFiles[i].Path))
                        {
                            writer.WriteStartObject();
                            writer.WriteString("path", RecentFiles[i].Path);
                            writer.WriteString("title", RecentFiles[i].Title);
                            writer.WriteEndObject();
                        }
                    }
                    writer.WriteEndArray();


                    writer.WriteEndObject();
                }

                File.WriteAllBytes(SettingsFilePath, stream.ToArray());
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
