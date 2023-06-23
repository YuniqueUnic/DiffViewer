using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffViewer.Managers;
using DiffViewer.Messages;
using DiffViewer.Models;
using DiffViewer.Views;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiffViewer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private ILogger _logger;
    private IDialogService _dialogService;
    private IWindow _aboutWindow;
    private IWindow _rawDataWindow;
    private int m_totalLineCount;

    public MainWindowViewModel(ILogger logger , IDialogService dialogService , params IWindow[] iWindows)
    {
        _logger = logger;
        _dialogService = dialogService;
        for( int i = 0; i < iWindows.Length; i++ )
        {
            _ = iWindows[i] switch
            {
                AboutWindow => _aboutWindow = iWindows[i],
                RawDataWindow => _rawDataWindow = iWindows[i],
                _ => throw new NotImplementedException(),
            };
        }

        logger.Information("MainWindowViewModel created");
    }

    [ObservableProperty]
    public TestCaseShare _testCaseShare = new();


    [ObservableProperty]
    public int[] _testCasesState;
    partial void OnTestCasesStateChanged(int[] value)
    {
        WeakReferenceMessenger.Default.Send<ShowBarchartMessage>(new ShowBarchartMessage()
        {
            Sender = this ,
            Message = "UpdateBarChart" ,
        });
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TestCasesState))]
    public ObservableCollection<TestCase> _diffTestCases;

    [ObservableProperty]
    string _searchText = "Search...";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TestCaseShare))]
    string _importedFileFullPath = "Import the Diff File...";

    [ObservableProperty]
    bool _isSideBySide = false;

    [ObservableProperty]
    string _OldResult = string.Empty;

    [ObservableProperty]
    string _NewResult = string.Empty;

    public int CurrentSelectedIndex = -1;

    [ObservableProperty]
    public int _selectedIndex = 0;
    partial void OnSelectedIndexChanging(int oldValue , int newValue) { if( newValue != -1 ) { CurrentSelectedIndex = newValue; } }

    [ObservableProperty]
    string _selectedTestCaseName;

    [ObservableProperty]
    public TestCase _selectedTestCase;

    [ObservableProperty]
    public SideBySideDiffModel _diffModel;

    [Obsolete("Too large memory consume.")]
    private void Compare( )
    {
        var diffBuilder = new SideBySideDiffBuilder(new Differ());
        DiffModel = diffBuilder.BuildDiffModel(OldResult ?? string.Empty , NewResult ?? string.Empty);
    }


    #region Window UI RelayCommands

    #region Logic

    [RelayCommand]
    public async Task ImportDiffFile(object doubleLeftClicked)
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
            TestCaseShare.Time = new FileInfo(settings.FileName).LastWriteTime.ToString();

            ImportedFileFullPath = settings.FileName;

            _logger.Information($"Imported Diff File: {ImportedFileFullPath}");

            await LoadDiffFile(ImportedFileFullPath);
        }
    }

    private async Task LoadDiffFile(string diffFilePath)
    {
        _logger.Debug("LoadDiffFileCommand called");
        DiffDataManager diffDataProvider = await new DiffDataManager(diffFilePath).HandleDiff();
        while( !diffDataProvider.IsProcessOver )
        {
            await Task.Delay(1000);
        }

        TestCasesState = diffDataProvider.TestCases.GroupBy(t => t.IsIdentical).Select(g => g.Count()).ToArray();

        DiffTestCases = new ObservableCollection<TestCase>(diffDataProvider.TestCases);



        //string WriteToPath = Path.Combine(Path.GetDirectoryName(diffFilePath) , Path.GetFileNameWithoutExtension(diffFilePath));
        //FileManager.WriteToAsync(diffDataProvider.DiffInfos.Item2 , WriteToPath + "\\DiffAll_" + Path.GetFileName(diffFilePath));

        //var a = diffDataProvider._diffNames.Aggregate((f , s) => { return string.Join(Environment.NewLine , f , s); });
        //FileManager.WriteToAsync(a , WriteToPath + "\\DiffNames_" + Path.GetFileName(diffFilePath));

        //var b = diffDataProvider.DiffResults.Aggregate((f , s) => { return string.Join(Environment.NewLine + "$".Repeat(64) + Environment.NewLine , f , s); });
        //FileManager.WriteToAsync(b , WriteToPath + "\\Results_" + Path.GetFileName(diffFilePath));
    }

    [RelayCommand]
    public void DoubleToSelectTestCase( )
    {
        _logger.Debug($"DoubleToSelectTestCaseCommand called,  TestCase Selected: {SelectedTestCase.Name}");
        ShowTestCaseDiff(SelectedTestCase);
    }

    public void ShowTestCaseDiff(TestCase testCase)
    {
        if( testCase is null ) return;
        _logger.Information($"ShowTestCaseDiff called, TestCase shows: {testCase.Name}");
        OldResult = testCase.OldText_BaseLine ?? string.Empty;
        NewResult = testCase.NewText_Actual ?? string.Empty;
        SelectedTestCaseName = testCase.Name;
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
    public void ShowSelectedTestCaseRawData( )
    {
        _logger.Debug("ShowSelectedTestCaseRawDataCommand called");
        if( DiffTestCases is null ) return;
        if( CurrentSelectedIndex < 0 || DiffTestCases[CurrentSelectedIndex] is null ) return;

        _rawDataWindow.Owner = App.ViewModelLocator.Main_Window;
        _rawDataWindow.DataContext = DiffTestCases[CurrentSelectedIndex];
        _rawDataWindow.Show();
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
