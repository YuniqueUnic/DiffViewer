using DiffViewer.Managers;
using DiffViewer.Services;
using DiffViewer.ViewModels;
using Serilog;
using System;
using System.IO;
using System.Windows;
using VSTSDataProvider.ViewModels;

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
        ViewModelLocator = (App.Current.FindResource("VMsLocator") as ViewModelLocator) ?? new();
        AppConfigManager.SwitchLanguageTo("zh-cn");
    }

    /// <summary>
    /// On language changed event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnLanguageChanged(object sender , LanguageChangedEventArgs e)
    {
        LoadLanguage(e);
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

        //try
        //{
        //    LanguageChanged?.Invoke(nameof(LanguageChanged) , new() { NewLangugae = e.NewLangugae , OldLanguage = e.OldLanguage });
        //}
        //catch( Exception ex )
        //{
        //    logger.Warning(ex , $"Failed to invoke language changed event.");
        //}
    }
}
public class LanguageChangedEventArgs : EventArgs
{
    public string? OldLanguage { get; set; }
    public string? NewLangugae { get; set; }
}

