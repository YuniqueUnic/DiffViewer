using DiffViewer.Managers;
using DiffViewer.Services;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
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

        ShowWindows();

        AppConfigManager.SwitchLanguageTo("zh-cn");
    }

    private void ShowWindows( )
    {
        ViewModelLocator = (App.Current.FindResource("VMsLocator") as ViewModelLocator) ?? new();

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


}
public class LanguageChangedEventArgs : EventArgs
{
    public string? OldLanguage { get; set; }
    public string? NewLangugae { get; set; }
}

