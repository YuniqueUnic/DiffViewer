using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DiffViewer.Managers;
using DiffViewer.Models;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace DiffViewer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private ILogger _logger;
    private IDialogService _dialogService;
    private IWindow _aboutWindow;
    private int m_totalLineCount;

    public MainWindowViewModel(ILogger logger , IDialogService dialogService , IWindow aboutWindow)
    {
        _logger = logger;
        _dialogService = dialogService;
        _aboutWindow = aboutWindow;
        logger.Information("MainWindowViewModel created");
    }

    [ObservableProperty]
    string _searchText = "Search...";

    [ObservableProperty]
    string _importedFileFullPath = "Import the Diff File...";

    [ObservableProperty]
    string _leftResult = string.Empty;

    [ObservableProperty]
    string _rightResult = string.Empty;


    [ObservableProperty]
    public ObservableCollection<TestCase> _diffTestCases;

    [ObservableProperty]
    public TestCase _selectedTestCase;

    #region Window UI RelayCommands

    #region Logic

    [RelayCommand]
    public async Task ImportDiffFile(Object doubleLeftClicked)
    {
        if( !(bool)doubleLeftClicked ) { return; }

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

        if( !File.Exists(ImportedFileFullPath) ) { return; }

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

        DiffTestCases = new ObservableCollection<TestCase>(diffDataProvider.TestCases);

        int a = 0;

        //string WriteToPath = Path.Combine(Path.GetDirectoryName(diffFilePath) , Path.GetFileNameWithoutExtension(diffFilePath));
        //FileManager.WriteToAsync(diffDataProvider.DiffInfos.Item2 , WriteToPath + "\\DiffAll_" + Path.GetFileName(diffFilePath));

        //var a = diffDataProvider._diffNames.Aggregate((f , s) => { return string.Join(Environment.NewLine , f , s); });
        //FileManager.WriteToAsync(a , WriteToPath + "\\DiffNames_" + Path.GetFileName(diffFilePath));

        //var b = diffDataProvider.DiffResults.Aggregate((f , s) => { return string.Join(Environment.NewLine + "$".Repeat(64) + Environment.NewLine , f , s); });
        //FileManager.WriteToAsync(b , WriteToPath + "\\Results_" + Path.GetFileName(diffFilePath));
    }

    [RelayCommand]
    public void TrytoSearchText( )
    {
        _logger.Debug("SearchCommand called");

    }


    [RelayCommand]
    public void ShowTestCaseDiff( )
    {
        if( SelectedTestCase is null ) return;
        _logger.Information($"ShowTestCaseDiff called, TestCase Selected: {SelectedTestCase.Name}");
        LeftResult = SelectedTestCase.OldText_BaseLine ?? string.Empty;
        RightResult = SelectedTestCase.NewText_Actual ?? string.Empty;
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
        _aboutWindow.Owner = App.ViewModelLocator.Main_Window;
        _aboutWindow.Show();
        //App.ViewModelLocator.About_Window.Show();
    }

    [RelayCommand]
    public void ShowUsageWindow( )
    {
        _logger.Debug($"{nameof(ShowUsageWindow)} called");
        _aboutWindow.Owner = App.ViewModelLocator.Main_Window;
        _aboutWindow.Show();
        //App.ViewModelLocator.About_Window.Show();
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
