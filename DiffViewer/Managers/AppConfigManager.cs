using DiffViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DiffViewer.Managers;

class DiffViewerAppConfig
{
    public string? ThemeName { get; set; }
    public string? AppLanguage { get; set; }
    public VSTSAccessInfo? AccessCode { get; set; }
}


class AppConfigManager
{
    public static string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) , "DiffViewer" , "DiffViewerFonfig.json");
    private static string _appLanguage = "en-us";
    public static string AppLanguage
    {
        get => _appLanguage;
        private set
        {
            if( !_appLanguage.Equals(value) )
            {
                App.LanguageChanged?.Invoke(nameof(AppConfigManager) , new()
                {
                    OldLanguage = _appLanguage ,
                    NewLangugae = value
                });

                _appLanguage = value;
            }
        }
    }
    public static string ThemeName { get; set; } = "HYSYS";
    public static string LanguageFolder { get; } = Path.Combine(Path.GetDirectoryName(ConfigPath),"Resources\\Languages");
    public static string LogPath { get; } = Path.Combine(Path.GetDirectoryName(ConfigPath),"Logs\\Log.txt");
    public static Dictionary<string , string> SupportLanguages { get; } = new() {
         { "zh-cn", "中文 (简体)" },
         { "en-us", "English (US)" },
    };

    public static VSTSAccessInfo AccessCode = new();

    public static void SwitchLanguageTo(string lang)
    {
        foreach( var item in SupportLanguages )
        {
            if( item.Key.Equals(lang) )
            {
                AppLanguage = lang;
                return;
            }
        }
    }

    /// <summary>
    /// Serialize the properties of AppConfigManager into JSON strings and save them to a local file
    /// </summary>
    /// <param name="fileFullPath"></param>
    public static void SaveConfigToFile(string fileFullPath)
    {
        if( !File.Exists(fileFullPath) )
        {
            // Create the directory if it does not exist
            Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(new DiffViewerAppConfig
        {
            ThemeName = AppConfigManager.ThemeName ,
            AppLanguage = AppConfigManager.AppLanguage ,
            AccessCode = AppConfigManager.AccessCode
        } , options);
        File.WriteAllText(fileFullPath , json , System.Text.Encoding.UTF8);

        App.Logger.Information("Save AppConfig Done. File Path {fileName}" , fileFullPath);
    }

    /// <summary>
    /// Read JSON strings from local files and deserialize them as properties of AppConfigManager
    /// </summary>
    /// <param name="fileName"></param>
    public static void LoadConfigFromFile(string fileName)
    {
        if( File.Exists(fileName) )
        {
            var json = File.ReadAllText(fileName , System.Text.Encoding.UTF8);
            var data = JsonSerializer.Deserialize<DiffViewerAppConfig>(json);
            AppConfigManager.ThemeName = data.ThemeName;
            AppConfigManager.AppLanguage = data.AppLanguage;
            AppConfigManager.AccessCode = data.AccessCode;
        }
        App.Logger.Information("Load AppConfig Done. File Path {fileName}" , fileName);
    }
}

