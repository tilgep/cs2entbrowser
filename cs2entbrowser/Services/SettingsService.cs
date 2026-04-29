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
    public string WorkshopFolder { get; private set; } = @"C:/Program Files (x86)/Steam/steamapps/workshop";
    public List<RecentFile> RecentFiles = new();
    public bool IsUsingDetailedSearch = false;
    public DoubleClickBehaviour DoubleClickBehaviour = DoubleClickBehaviour.Jump;

    private bool _rawProperties = false;
    public bool RawProperties
    {
        get => _rawProperties;
        set => this.RaiseAndSetIfChanged(ref _rawProperties, value);
    }

    public string IOOutput { get; private set; } = "outputname";
    public string IOTarget { get; private set; } = "targetname";
    public string IOInput { get; private set; } = "inputname";
    public string IOParam { get; private set; } = "overrideparam";
    public string IODelay { get; private set; } = "delay";
    public string IOTTF { get; private set; } = "timestofire";


    public void SetWorkshopFolder(string folder)
    {
        WorkshopFolder = folder;
        WriteSettings();
    }

    public void SetRawProperties(bool state)
    {
        RawProperties = state;
        WriteSettings();
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

    public void SetIO(string o, string t, string i, string p, string d, string ttf)
    {
        IOOutput = o;
        IOTarget = t;
        IOInput = i;
        IOParam = p;
        IODelay = d;
        IOTTF = ttf;
        WriteSettings();
        VpkService.Instance.DirtyIO = !VpkService.Instance.DirtyIO;
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

            WorkshopFolder = ReadSettingString(root, nameof(WorkshopFolder), WorkshopFolder);

            IsUsingDetailedSearch = ReadSettingBool(root, nameof(IsUsingDetailedSearch), IsUsingDetailedSearch);

            DoubleClickBehaviour = (DoubleClickBehaviour)ReadSettingNumber(root, nameof(DoubleClickBehaviour), (int)DoubleClickBehaviour);

            RawProperties = ReadSettingBool(root, nameof(RawProperties), RawProperties);

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

            IOOutput = ReadSettingString(root, nameof(IOOutput), IOOutput);
            IOTarget = ReadSettingString(root, nameof(IOTarget), IOTarget);
            IOInput = ReadSettingString(root, nameof(IOInput), IOInput);
            IOParam = ReadSettingString(root, nameof(IOParam), IOParam);
            IODelay = ReadSettingString(root, nameof(IODelay), IODelay);
            IOTTF = ReadSettingString(root, nameof(IOTTF), IOTTF);

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
                    writer.WriteBoolean(nameof(RawProperties), RawProperties);

                    writer.WriteString(nameof(IOOutput), IOOutput);
                    writer.WriteString(nameof(IOTarget), IOTarget);
                    writer.WriteString(nameof(IOInput), IOInput);
                    writer.WriteString(nameof(IOParam), IOParam);
                    writer.WriteString(nameof(IODelay), IODelay);
                    writer.WriteString(nameof(IOTTF), IOTTF);

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

    private string ReadSettingString(JsonElement root, string keyName, string defaultValue = "")
    {
        string val = defaultValue;
        if (root.TryGetProperty(keyName, out JsonElement elem))
        {
            string? elemString = elem.GetString();
            if (elemString != null)
            {
                val = elemString;
            }
        }
        return val;
    }

    private bool ReadSettingBool(JsonElement root, string keyName, bool defaultValue = false)
    {
        bool val = defaultValue;
        if (root.TryGetProperty(keyName, out JsonElement elem))
        {
            if (elem.ValueKind == JsonValueKind.True || elem.ValueKind == JsonValueKind.False)
                val = elem.GetBoolean();
        }
        return val;
    }

    private int ReadSettingNumber(JsonElement root, string keyName, int defaultValue = 0)
    {
        int val = defaultValue;
        if (root.TryGetProperty(keyName, out JsonElement elem))
        {
            if (elem.ValueKind == JsonValueKind.Number && elem.TryGetInt32(out int elemValue))
            {
                val = elemValue;
            }
        }
        return val;
    }
}
