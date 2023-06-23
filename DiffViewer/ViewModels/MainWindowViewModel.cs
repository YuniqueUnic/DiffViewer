using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffViewer.Managers;
using DiffViewer.Managers.Helper;
using DiffViewer.Messages;
using DiffViewer.Models;
using DiffViewer.Views;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private string m_ExportFileFullPath;
    private IEnumerable<IGrouping<bool? , TestCase>> m_GroupedTestCases;

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
    public int[] _testCasesState = new int[3];
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


        m_GroupedTestCases = diffDataProvider.TestCases.GroupBy(t => t.IsIdentical);

        // TestCasesState[0] = identicalCount;
        TestCasesState[0] = m_GroupedTestCases
                             .Where(g => g.Key.HasValue && g.Key.Value)
                             .Sum(g => g.Count());

        // TestCasesState[1] = nonIdenticalCount;
        TestCasesState[1] = m_GroupedTestCases
                                .Where(g => g.Key.HasValue && !g.Key.Value)
                                .Sum(g => g.Count());

        // TestCasesState[2] = errorCount;
        TestCasesState[2] = m_GroupedTestCases
                         .Where(g => !g.Key.HasValue)
                         .Sum(g => g.Count());


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
        SelectedTestCaseName = testCase.Name ?? string.Empty;
    }




    [RelayCommand]
    public async Task ExportPassedExcel( )
    {
        _logger.Information($"({nameof(ExportFailNullLst)} Called)");

        var settings = new SaveFileDialogSettings()
        {
            Title = $"{App.Current.Resources.MergedDictionaries[0]["ExportPassDescription"]}" ,
            Filter = "Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv|All (*.*)|*.*" ,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) ,
            CheckPathExists = true ,
            AddExtension = true ,
            FileName = "OTE_" + ImportedFileFullPath.GetFileName(withoutExt: true) + DateTime.Now.ToString("MM_dd_yy_HH") ,
            DefaultExt = "xlsx" ,

        };

        bool? success = _dialogService.ShowSaveFileDialog(this , settings);

        if( success == true )
        {
            m_ExportFileFullPath = settings.FileName;

            await ExportPassedExcelAsync(m_ExportFileFullPath , m_GroupedTestCases);
        }
    }

    private async Task ExportPassedExcelAsync(string m_ExportFileFullPath , IEnumerable<IGrouping<bool? , TestCase>> m_GroupedTestCases)
    {
        if( m_GroupedTestCases is null ) { return; }

        var location = $"({nameof(ExportFailNullLstAsync)} Called).(Export File Full Path: {m_ExportFileFullPath})";

        await TasksManager.RunTaskAsync(async ( ) =>
        {
            await m_GroupedTestCases.Where(g => g.Key != true)
                                  .SelectMany(g => g)
                                  .Select(t => t.Name)
                                  .WriteStringsToAsync(m_ExportFileFullPath);

        } , location , catchException: true);

        MessageBoxSettings messageBoxSettings = new()
        {
            Button = System.Windows.MessageBoxButton.YesNo ,
            Caption = App.Current.Resources.MergedDictionaries[0]["SucceedExport"].ToString() ?? "Succeed to Export" ,
            Icon = System.Windows.MessageBoxImage.Information ,
            DefaultResult = System.Windows.MessageBoxResult.Yes ,
            Options = System.Windows.MessageBoxOptions.None ,
            MessageBoxText = App.Current.Resources.MergedDictionaries[0]["ExportFailNullDescription"].ToString()
                             + Environment.NewLine
                             + Environment.NewLine
                             + m_ExportFileFullPath
                             + Environment.NewLine
                             + App.Current.Resources.MergedDictionaries[0]["ClickYesToOpen"].ToString()
                             ?? $"Yes to open the directory of it." ,
        };

        System.Windows.MessageBoxResult msgResult = _dialogService.ShowMessageBox(this , messageBoxSettings);

        if( msgResult == System.Windows.MessageBoxResult.Yes )
        {
            try
            {
                Process.Start("explorer.exe" , $"/select,\"{m_ExportFileFullPath}\"");
                _logger.Information($"Explorer.exe launched to show the file: {m_ExportFileFullPath}.");
            }
            catch( Exception ex )
            {
                _logger.Error($"Error On lauching the explorer.exe to show the file: {m_ExportFileFullPath}." +
                              $"{Environment.NewLine}Exception: {ex.Message}");
                throw;
            }
        }

    }


    [RelayCommand]
    public async Task ExportFailNullLst( )
    {
        _logger.Information($"({nameof(ExportFailNullLst)} Called)");

        var settings = new SaveFileDialogSettings()
        {
            Title = $"{App.Current.Resources.MergedDictionaries[0]["ExportFailNullDescription"]}" ,
            Filter = "Lst File (*.lst)|*.lst|All (*.*)|*.*" ,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) ,
            FileName = ImportedFileFullPath.GetFileName(withoutExt: true) + DateTime.Now.ToString("MM_dd_yy_HH") ,
            DefaultExt = "lst" ,
            CheckPathExists = true ,
            AddExtension = true ,
        };

        bool? success = _dialogService.ShowSaveFileDialog(this , settings);

        if( success == true )
        {
            m_ExportFileFullPath = settings.FileName;

            await ExportFailNullLstAsync(m_ExportFileFullPath , m_GroupedTestCases);
        }
    }

    private async Task ExportFailNullLstAsync(string exportFileFullPath , IEnumerable<IGrouping<bool? , TestCase>> groupedTestCases)
    {
        if( groupedTestCases is null ) { return; }

        var location = $"({nameof(ExportFailNullLstAsync)} Called).(Export File Full Path: {exportFileFullPath})";

        await TasksManager.RunTaskAsync(async ( ) =>
        {
            await groupedTestCases.Where(g => g.Key != true)
                                  .SelectMany(g => g)
                                  .Select(t => t.Name)
                                  .WriteStringsToAsync(exportFileFullPath);

        } , location , catchException: true);

        MessageBoxSettings messageBoxSettings = new()
        {
            Button = System.Windows.MessageBoxButton.YesNo ,
            Caption = App.Current.Resources.MergedDictionaries[0]["SucceedExport"].ToString() ?? "Succeed to Export" ,
            Icon = System.Windows.MessageBoxImage.Information ,
            DefaultResult = System.Windows.MessageBoxResult.Yes ,
            Options = System.Windows.MessageBoxOptions.None ,
            MessageBoxText = App.Current.Resources.MergedDictionaries[0]["ExportFailNullDescription"].ToString()
                             + Environment.NewLine
                             + Environment.NewLine
                             + exportFileFullPath
                             + Environment.NewLine
                             + App.Current.Resources.MergedDictionaries[0]["ClickYesToOpen"].ToString()
                             ?? $"Yes to open the directory of it." ,
        };

        System.Windows.MessageBoxResult msgResult = _dialogService.ShowMessageBox(this , messageBoxSettings);

        if( msgResult == System.Windows.MessageBoxResult.Yes )
        {
            try
            {
                Process.Start("explorer.exe" , $"/select,\"{exportFileFullPath}\"");
                _logger.Information($"Explorer.exe launched to show the file: {exportFileFullPath}.");
            }
            catch( Exception ex )
            {
                _logger.Error($"Error On lauching the explorer.exe to show the file: {exportFileFullPath}." +
                              $"{Environment.NewLine}Exception: {ex.Message}");
                throw;
            }
        }

    }


    //Refactoring
    private async Task ShowSaveDialog(string title , string filter , string defaultExt , string initialDirectory , string fileName , Func<IGrouping<bool? , TestCase> , bool> filterPredicate)
    {
        _logger.Information($"({nameof(ShowSaveDialog)} Called)");

        var settings = new SaveFileDialogSettings()
        {
            Title = title ,
            Filter = filter ,
            InitialDirectory = initialDirectory ,
            CheckPathExists = true ,
            AddExtension = true ,
            FileName = fileName ,
            DefaultExt = defaultExt
        };

        bool? success = _dialogService.ShowSaveFileDialog(this , settings);

        if( success == true )
        {
            m_ExportFileFullPath = settings.FileName;

            await ExportToFileAsync(m_ExportFileFullPath , m_GroupedTestCases , filterPredicate);
        }
    }

    private async Task ExportToFileAsync(string exportFileFullPath , IEnumerable<IGrouping<bool? , TestCase>> groupedTestCases , Func<IGrouping<bool? , TestCase> , bool> filterPredicate)
    {
        if( groupedTestCases is null ) { return; }

        var location = $"({nameof(ExportToFileAsync)} Called).(Export File Full Path: {exportFileFullPath})";

        await TasksManager.RunTaskAsync(async ( ) =>
        {
            await groupedTestCases.Where(filterPredicate)
                                  .SelectMany(g => g)
                                  .Select(t => t.Name)
                                  .WriteStringsToAsync(exportFileFullPath);

        } , location , catchException: true);

        MessageBoxSettings messageBoxSettings = new()
        {
            Button = System.Windows.MessageBoxButton.YesNo ,
            Caption = App.Current.Resources.MergedDictionaries[0]["SucceedExport"].ToString() ?? "Succeed to Export" ,
            Icon = System.Windows.MessageBoxImage.Information ,
            DefaultResult = System.Windows.MessageBoxResult.Yes ,
            Options = System.Windows.MessageBoxOptions.None ,
            MessageBoxText = App.Current.Resources.MergedDictionaries[0]["ExportFailNullDescription"].ToString()
                             + Environment.NewLine
                             + Environment.NewLine
                             + exportFileFullPath
                             + Environment.NewLine
                             + App.Current.Resources.MergedDictionaries[0]["ClickYesToOpen"].ToString()
                             ?? $"Yes to open the directory of it." ,
        };

        System.Windows.MessageBoxResult msgResult = _dialogService.ShowMessageBox(this , messageBoxSettings);

        if( msgResult == System.Windows.MessageBoxResult.Yes )
        {
            try
            {
                Process.Start("explorer.exe" , $"/select,\"{exportFileFullPath}\"");
                _logger.Information($"Explorer.exe launched to show the file: {exportFileFullPath}.");
            }
            catch( Exception ex )
            {
                _logger.Error($"Error On lauching the explorer.exe to show the file: {exportFileFullPath}." +
                              $"{Environment.NewLine}Exception: {ex.Message}");
                throw;
            }
        }

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
