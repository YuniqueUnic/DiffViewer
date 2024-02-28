using DiffViewer.Managers;
using DiffViewer.Managers.Helper;
using DiffViewer.Services;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DiffViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    public static EventHandler<LanguageChangedEventArgs> LanguageChanged;

    /// <summary>
    /// ViewModelLocator for all ViewModels. Ioc Container
    /// </summary>
    public static ViewModelLocator ViewModelLocator { get; set; }
    public static ILogger Logger => ViewModelLocator.Logger;
    public App( )
    {
        InitializeComponent();

        LanguageChanged += OnLanguageChanged;

        // Build ViewModelLocator IOC
        ViewModelLocator = (App.Current.FindResource("VMsLocator") as ViewModelLocator) ?? new();

        LoadAppConfiguration();

        LoadLanguageFromConfig();

        ShowWindows();
    }


    internal static void LoadAppConfiguration( )
    {
        if( !File.Exists(AppConfigManager.ConfigPath) )
        {
            AppConfigManager.SaveConfigToFile(AppConfigManager.ConfigPath);
        }

        AppConfigManager.LoadConfigFromFile(AppConfigManager.ConfigPath);
        Logger.Information("AppConfig was successfully Loaded. File Path: {filePath}" , AppConfigManager.ConfigPath);
    }

    internal static void SaveAppConfiguration( )
    {
        AppConfigManager.SaveConfigToFile(AppConfigManager.ConfigPath);
        Logger.Information("AppConfig was successfully saved. File Path: {filePath}" , AppConfigManager.ConfigPath);
    }

    internal void LoadLanguageFromConfig( )
    {
        string lang = AppConfigManager.AppLanguage.IsNullOrWhiteSpaceOrEmpty() ? "en-us" : AppConfigManager.AppLanguage;

        AppConfigManager.SwitchLanguageTo(lang);

        LanguageChanged.Invoke(this , new() { OldLanguage = "" , NewLangugae = lang });
    }

    private void ShowWindows( )
    {
        ViewModelLocator.Main_Window.Show();
    }

    /// <summary>
    /// On language changed event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnLanguageChanged(object sender , LanguageChangedEventArgs e)
    {
        SwitchLanguageCulture(e);
        LoadLanguage(e);
    }

    /// <summary>
    /// Set language for application.
    /// </summary>
    /// <param name="language"></param>
    public static void SwitchLanguageCulture(LanguageChangedEventArgs e)
    {
        string language = e.NewLangugae ?? "en-us";
        // Create a CultureInfo instance for the specified culture.
        CultureInfo culture = new CultureInfo(language);

        // Set current culture for all threads.
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    /// <summary>
    /// Load language file and change language.
    /// </summary>
    private static void LoadLanguage(LanguageChangedEventArgs e)
    {
        ILogger logger = ViewModelLocator.Logger!;
        logger.Information($"Trying to switch language from {e.OldLanguage} to {e.NewLangugae}.");
        
        var path = Path.GetFullPath($"{AppConfigManager.LanguageFolder}/{e.NewLangugae}.xaml");
        var backup_langpath = Path.GetFullPath($"{AppConfigManager.LanguageFolder}/{e.OldLanguage}.xaml");
        if (!File.Exists(path))
        {
            string directoryPath = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directoryPath);
            using var langStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"DiffViewer.Resources.Languages.{e.NewLangugae}.xaml");
            using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                langStream.CopyTo(fileStream);
            }
        }
        var uri = new Uri(path , UriKind.RelativeOrAbsolute);
        var backup_languri = new Uri(backup_langpath , UriKind.RelativeOrAbsolute);

        try
        {
            App.Current.Resources.MergedDictionaries.Clear();
            var dictionary = new ResourceDictionary() { Source = uri };
            App.Current.Resources.MergedDictionaries.Add(dictionary);
        }
        catch( Exception ex )
        {
            logger.Warning(ex , $"Language File {e.NewLangugae}.xaml not found.");

            App.Current.Resources.MergedDictionaries.Clear();

            try
            {
                var dictionary = new ResourceDictionary() { Source = backup_languri };
                Application.Current.Resources.MergedDictionaries.Add(dictionary);
                AppConfigManager.SwitchLanguageTo(e.OldLanguage ?? "en-us");
            }
            catch( Exception exp )
            {
                logger.Warning(exp , $"Suspected absence of language files on record.");
            }
        }
        finally
        {
            logger.Information($"Language changed from {e.OldLanguage} to {e.NewLangugae} successfully.");
        }

    }

    /// <summary>
    /// Save App Config before app shutdown
    /// </summary>
    /// <param name="e"></param>
    protected override void OnExit(ExitEventArgs e)
    {
        SaveAppConfiguration();
        base.OnExit(e);
    }
}
public class LanguageChangedEventArgs : EventArgs
{
    public string? OldLanguage { get; set; }
    public string? NewLangugae { get; set; }
}

