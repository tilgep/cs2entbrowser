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

public enum DoubleClickBehaviour
{
    None,
    Jump,
    Search,
}
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
    public static int VERSION = 2;
    public static SettingsService Instance { get; } = new();
    public static string SettingsFolder = "CS2EntBrowser";
    public static string SettingsFileName = "settings.json";

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
    public DoubleClickBehaviour DoubleClickBehaviour = DoubleClickBehaviour.Jump;

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
            using JsonDocument doc = JsonDocument.Parse(jsonString);

            JsonElement root = doc.RootElement;

            Debug.WriteLine("Reading version");
            if (root.TryGetProperty(nameof(VERSION), out JsonElement versionElem))
            { 
                if (versionElem.ValueKind == JsonValueKind.Number && versionElem.TryGetInt32(out int fileVersion))
                {
                    Debug.WriteLine("Settings Config Version: " + fileVersion.ToString());
                    Debug.WriteLine("Current App Version: " + VERSION.ToString());
                } 
            }

            Debug.WriteLine("Reading folder");
            if (root.TryGetProperty(nameof(WorkshopFolder), out JsonElement folderElem))
            {
                string? folder = folderElem.GetString();
                if (folder != null)
                {
                    WorkshopFolder = folder;
                }
            }

            Debug.WriteLine("Reading detailed search");
            if (root.TryGetProperty(nameof(IsUsingDetailedSearch), out JsonElement searchElem))
            {
                if(searchElem.ValueKind == JsonValueKind.True || searchElem.ValueKind == JsonValueKind.False)
                    IsUsingDetailedSearch = searchElem.GetBoolean();
            }

            Debug.WriteLine("Reading double click");
            if (root.TryGetProperty(nameof(DoubleClickBehaviour), out JsonElement clickElem))
            {
                if (clickElem.ValueKind == JsonValueKind.Number && clickElem.TryGetInt32(out int clickValue))
                {
                    DoubleClickBehaviour = (DoubleClickBehaviour)clickValue;
                }
            }

            Debug.WriteLine("Reading recent files");
            if (root.TryGetProperty(nameof(RecentFiles), out JsonElement recentFilesElem))
            {
                for (int i = 0; i < recentFilesElem.GetArrayLength(); i++)
                {
                    JsonElement recentFileObj = recentFilesElem[i];
                    string? path = null;
                    string? title = null;

                    if (recentFileObj.TryGetProperty("path", out JsonElement pathElem))
                    {
                        path = pathElem.GetString();
                    }

                    if (recentFileObj.TryGetProperty("title", out JsonElement titleElem))
                    {
                        title = titleElem.GetString();
                    }

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

            Debug.WriteLine("Reading DONE");
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
                    writer.WriteNumber(nameof(VERSION), VERSION);
                    writer.WriteString(nameof(WorkshopFolder), WorkshopFolder);
                    writer.WriteBoolean(nameof(IsUsingDetailedSearch), IsUsingDetailedSearch);
                    writer.WriteNumber(nameof(DoubleClickBehaviour), (int)DoubleClickBehaviour);

                    writer.WriteStartArray(nameof(RecentFiles));
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
