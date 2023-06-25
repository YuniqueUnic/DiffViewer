using DiffViewer.Models;
using System.Collections.Generic;

namespace DiffViewer.Managers;

class AppConfigManager
{
    private static string _appLanguage = "";
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
    public static string ThemeName { get; } = "HYSYS";
    public static string LanguageFolder { get; } = "./Resources/Languages";
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
}

