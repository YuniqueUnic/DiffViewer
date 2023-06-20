using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Managers;
using DiffViewer.Managers.Helper;
using DiffViewer.Views;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiffViewer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private ILogger _logger;
    private IDialogService _dialogService;
    private int m_totalLineCount;

    public MainWindowViewModel(ILogger logger , IDialogService dialogService)
    {
        _logger = logger;
        _dialogService = dialogService;
        logger.Information("MainWindowViewModel created");
    }

    [ObservableProperty]
    string _searchText = "Search...";
    //string _searchText = $"{App.Current.Resources.MergedDictionaries[0]["Search"]}...";

    [ObservableProperty]
    string _importedFileFullPath = "Import the Diff File...";


    #region Window UI RelayCommands

    #region Logic

    [RelayCommand]
    public async Task ImportDiffFile( )
    {
        _logger.Debug("ImportDiffFileCommand called");

        var settings = new OpenFileDialogSettings()
        {
            Title = $"{App.Current.Resources.MergedDictionaries[0]["Import"]} Diff" ,
            Filter = "Dif File (*.dif)|*.dif|All (*.*)|*.*" ,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) ,
            CheckFileExists = true ,
        };

        bool? success = _dialogService.ShowOpenFileDialog(this , settings);

        if( success == true )
        {
            ImportedFileFullPath = settings.FileName;
            _logger.Information($"Imported Diff File: {ImportedFileFullPath}");
        }

        await LoadDiffFile(ImportedFileFullPath);

    }

    private async Task LoadDiffFile(string diffFilePath)
    {
        _logger.Debug("LoadDiffFileCommand called");
        DiffDataManager diffDataProvider = await new DiffDataManager(diffFilePath).HandleDiff();
        while( !diffDataProvider.IsProcessOver )
        {
            await Task.Delay(1000);
        }

        string WriteToPath = Path.Combine(Path.GetDirectoryName(diffFilePath) , Path.GetFileNameWithoutExtension(diffFilePath));
        FileManager.WriteToAsync(diffDataProvider.DiffInfos.Item2 , WriteToPath + "\\DiffAll_" + Path.GetFileName(diffFilePath));

        var a = diffDataProvider.DiffNames.Aggregate((f , s) => { return string.Join(Environment.NewLine , f , s); });
        FileManager.WriteToAsync(a , WriteToPath + "\\DiffNames_" + Path.GetFileName(diffFilePath));

        var b = diffDataProvider.DiffResults.Aggregate((f , s) => { return string.Join(Environment.NewLine + "$".Repeat(64) + Environment.NewLine , f , s); });
        FileManager.WriteToAsync(b , WriteToPath + "\\Results_" + Path.GetFileName(diffFilePath));
    }


    #endregion Logic

    #region UI

    [RelayCommand]
    public void SwitchLanguage(string lang)
    {
        _logger.Debug("SwitchLanguageCommand called");
        AppConfigManager.SwitchLanguageTo(lang);
    }

    [RelayCommand]
    public void ShowAboutWindow( )
    {
        _logger.Debug("ShowAboutWindowCommand called");
        WeakReferenceMessenger.Default.Send(new Messages.WindowActionMessage() { Sender = this , Message = $"Show{nameof(AboutWindow)}" });
        App.ViewModelLocator.About_Window.Show();
    }

    /// <summary>
    /// Close Window by using WeakReferenceMessenger.
    /// </summary>
    [RelayCommand]
    public void CloseWindow( )
    {
        _logger.Debug("CloseWindowCommand called");
        WeakReferenceMessenger.Default.Send(new Messages.WindowActionMessage() { Sender = this , Message = "Close" });
    }

    /// <summary>
    /// Maximize Window by using WeakReferenceMessenger.
    /// </summary>
    [RelayCommand]
    public void MaximizeWindow( )
    {
        _logger.Debug("MaximizeWindowCommand called");
        WeakReferenceMessenger.Default.Send(new Messages.WindowActionMessage() { Sender = this , Message = "Maximize" });
    }

    /// <summary>
    /// Minimize Window by using WeakReferenceMessenger.
    /// </summary>
    [RelayCommand]
    public void MinimizeWindow( )
    {
        _logger.Debug("MinimizeWindowCommand called");
        WeakReferenceMessenger.Default.Send(new Messages.WindowActionMessage() { Sender = this , Message = "Minimize" });
    }

    #endregion UI

    #endregion
}
