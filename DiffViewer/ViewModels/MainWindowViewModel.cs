using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
using System.Collections.Concurrent;
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
    private IWindow _vstsSettingWindow;

    private string m_ExportFileFullPath;
    public string LatestExportDirctory
    {
        get
        {
            if( m_ExportFileFullPath.IsNullOrWhiteSpaceOrEmpty() )
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            return Path.GetDirectoryName(m_ExportFileFullPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }

    private IEnumerable<IGrouping<bool? , DiffTestCase>> m_GroupedTestCases;

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
                VSTSSettingWindow => _vstsSettingWindow = iWindows[i],
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
        WeakReferenceMessenger.Default.Send<UpdateBarChartMessage>(new UpdateBarChartMessage()
        {
            Sender = this ,
            Message = "UpdateBarChart" ,
        });
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TestCasesState) , nameof(TestCasesIndexes))]
    public ObservableCollection<DiffTestCase> _diffTestCases = new ObservableCollection<DiffTestCase>();

    public IEnumerable<int> TestCasesIndexes => Enumerable.Range(1 , DiffTestCases.Count);


    [ObservableProperty]
    public ConcurrentBag<OTETestCase> _OTETestCases;


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

    [ObservableProperty]
    public int _currentSelectedIndex = -1;

    [ObservableProperty]
    public int _selectedIndex = -1;
    partial void OnSelectedIndexChanging(int oldValue , int newValue) { if( newValue != -1 ) { CurrentSelectedIndex = newValue; } }

    [ObservableProperty]
    string _selectedTestCaseName;

    [ObservableProperty]
    public DiffTestCase _selectedTestCase;

    [ObservableProperty]
    public SideBySideDiffModel _diffModel;

    //[Obsolete("Too large memory consume.")]
    //private void Compare( )
    //{
    //    var diffBuilder = new SideBySideDiffBuilder(new Differ());
    //    DiffModel = diffBuilder.BuildDiffModel(OldResult ?? string.Empty , NewResult ?? string.Empty);
    //}


    /// <summary>
    /// End of handling => True
    /// In progress     => null
    /// Start to handle => False
    /// </summary>
    [ObservableProperty]
    public bool? _isVSTSDataHandleOver = false;



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


        DiffTestCases = new ObservableCollection<DiffTestCase>(diffDataProvider.TestCases);



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
        if( SelectedTestCase is null ) return;
        _logger.Debug($"DoubleToSelectTestCaseCommand called,  TestCase Selected: {SelectedTestCase.Name}");
        ShowTestCaseDiff(SelectedTestCase);
    }

    public void ShowTestCaseDiff(DiffTestCase testCase)
    {
        if( testCase is null ) return;
        _logger.Information($"ShowTestCaseDiff called, TestCase shows: {testCase.Name}");
        OldResult = testCase.OldText_BaseLine ?? string.Empty;
        NewResult = testCase.NewText_Actual ?? string.Empty;
        SelectedTestCaseName = testCase.Name ?? string.Empty;
    }




    [RelayCommand]
    public async Task ExportPassedScriptsToExcel( )
    {
        _logger.Debug("ExportPassedScriptsToExcelCommand called");

        if( !VSTSDataManager.IsValidUrl(AppConfigManager.AccessCode.Url) )
        {
            string caption = App.Current.Resources.MergedDictionaries[0]["PleaseSetUriFirst"].ToString() ?? "Please set the VSTS URL first.";
            string msgText = caption + Environment.NewLine.Repeat(2) + App.Current.Resources.MergedDictionaries[0]["ErrorAccessCodeInfo"].ToString() ?? "Token or Cookie must be filled in.";
            ShowExportResultMessageBox(caption , msgText , false , string.Empty);
            return;
        }

        if ( m_GroupedTestCases is null )
        {
            _logger.Warning("No Diff data got.");
            return;
        }

        _logger.Information("Start trying to Get OTE TestCases from VSTS.");

        await GetOTETestCasesAsync(() => { IsVSTSDataHandleOver = null; }); 
       

        //IEnumerable<string?> nonIdenticalScripts = m_GroupedTestCases.Where(g => g.Key != true)
        //                                                       .SelectMany(g => g)
        //                                                       .Select(t => t.Name);

        //_logger.Information("Start trying to export Passed OTE TestCases to Excel.");
        //LogResultExceptNonIdenticalScripts(nonIdenticalScripts, OTETestCases);

        IEnumerable<string?> passedScripts = m_GroupedTestCases.Where(g => g.Key == true)
                                                               .SelectMany(g => g)
                                                               .Select(t => t.Name);

        _logger.Information("Start trying to export Passed OTE TestCases to Excel.");
        LogResultForPassedScripts(passedScripts, OTETestCases);

        await ExportPassToExcelLogicAsync();

        IsVSTSDataHandleOver = true;
    }

    public async Task GetOTETestCasesAsync(Action afterPreLoadDataAction=null)
    {
        var OTETCs = await VSTSDataManager.GET_OTETestCasesAsync(AppConfigManager.AccessCode, afterPreLoadDataAction);
        OTETestCases = OTETCs;
    }

    // LogResultExceptNonIdenticalScripts is better than LogResultForPassedScripts normally, but for different situation, it may have different performance.
    // 1. Set all scripts except the Failed to Passed first.
    // 2. Set the string.Empty for non-identical scripts.
    public bool LogResultExceptNonIdenticalScripts(IEnumerable<string?> nonIdenticalScripts, ConcurrentBag<OTETestCase> oTETestCases)
    {
        bool success = false;

        oTETestCases.AsParallel().ForAll(tc => {if (tc.Outcome != "Failed"){tc.Outcome = "Passed";}});

        foreach (string scriptName in nonIdenticalScripts)
        {
            const string scpEx = @".scp";
            const string hysysPrefix = @"hytest: ";
            string lowerScriptName = scriptName.ToLowerInvariant().Trim();

            OTETestCase matchingTestCase = oTETestCases.FirstOrDefault(t =>
            {
                bool isMatch = t.GetScriptName().ToLowerInvariant().Replace(scpEx, string.Empty).Trim().Equals(lowerScriptName);
                if (isMatch) { return true; }
                return t.Title.ToLowerInvariant().Replace(hysysPrefix, string.Empty).Trim().Equals(lowerScriptName);
            });

            if (matchingTestCase is not null)
            {
                //matchingTestCase.Outcome = "Passed";
                matchingTestCase.Outcome = string.Empty;
                success = true;
            }
        }

        return success;
    }

    // Since the script name or test case name will have a spelling or alias, it will not correctly distinguish between all incoming scripts.
    // try to use logresultexceptnonidentialscripts
    // 1. Set all scripts except the Failed to Passed first.
    // 2. Set the string.Empty for non-identical scripts.
    // LogResultExceptNonIdenticalScripts is better than LogResultForPassedScripts normally, but for different situation, it may have different performance.
    public bool LogResultForPassedScripts(IEnumerable<string?> passedScripts , ConcurrentBag<OTETestCase> oTETestCases)
    {
        bool success = false;

        foreach( string scriptName in passedScripts )
        {
            const string scpEx = @".scp";
            const string hysysPrefix = @"hytest: ";
            string lowerScriptName = scriptName.ToLowerInvariant().Trim();

            OTETestCase matchingTestCase = oTETestCases.FirstOrDefault(t =>
            {
                bool isMatch = t.GetScriptName().ToLowerInvariant().Replace(scpEx, string.Empty).Trim().Equals(lowerScriptName);
                if (isMatch) { return true; }
                return t.Title.ToLowerInvariant().Replace(hysysPrefix, string.Empty).Trim().Equals(lowerScriptName);
            });

            if ( matchingTestCase is not null )
            {
                matchingTestCase.Outcome = "Passed";
                //matchingTestCase.Outcome = "Modify😀";
                success = true;
            }
        }

        return success;
    }

    public async Task ExportPassToExcelLogicAsync( )
    {
        _logger.Information($"({nameof(ExportPassToExcelLogicAsync)} Called)");

        string title = $"{App.Current.Resources.MergedDictionaries[0]["ExportPassToExcelDescription"]}";
        string filter = "Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv|All (*.*)|*.*";
        string defaultExt = "xlsx";
        string initialDirectory = LatestExportDirctory;

        string initialFileNameWithMoreInfo = ConcatMoreInfoToFileName("OTE_" + ImportedFileFullPath.GetFileName(withoutExt: true) , appendTimeNow: true);

        Action action = async ( ) =>
        {
            _logger.Information($"Start to Export Excel to {m_ExportFileFullPath}.");
            var result = await FileManager.ExportToExcelAsync<ConcurrentBag<OTETestCase>>(m_ExportFileFullPath , OTETestCases);

            // Show Export Result MessageBox 
            string _messageBoxCaption;
            string _msgBoxText;

            if( result.SucceedDone )
            {
                _messageBoxCaption = App.Current.Resources.MergedDictionaries[0]["SucceedExport"].ToString() ?? "Export Result"; ;
                _msgBoxText = (App.Current.Resources.MergedDictionaries[0]["ExportPassToExcelDescription"].ToString() ?? "Export Identical to excel successfully.")
                            + Environment.NewLine
                            + Environment.NewLine
                            + m_ExportFileFullPath
                            + Environment.NewLine
                            + App.Current.Resources.MergedDictionaries[0]["ClickYesToOpen"].ToString()
                            ?? $"Yes to open the directory of it.";
            }
            else
            {
                _messageBoxCaption = App.Current.Resources.MergedDictionaries[0]["FailedExport"].ToString() ?? "Export Result";
                _msgBoxText = _messageBoxCaption
                            + Environment.NewLine
                            + Environment.NewLine
                            + m_ExportFileFullPath
                            + Environment.NewLine;
            }

            _logger.Information("Start to show MessageBox.");
            ShowExportResultMessageBox(_messageBoxCaption , _msgBoxText , result.SucceedDone , m_ExportFileFullPath);
        };

        await ShowSaveDialog(title , filter , defaultExt , initialDirectory , initialFileNameWithMoreInfo , action);
    }



    [RelayCommand]
    public async Task ExportPassedLst( )
    {

        _logger.Information($"({nameof(ExportPassedLst)} Called)");

        string title = $"{App.Current.Resources.MergedDictionaries[0]["ExportPassToLstDescription"]}";
        string filter = "Lst File (*.lst)|*.lst|Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv|All (*.*)|*.*";
        string defaultExt = "lst";
        string initialDirectory = LatestExportDirctory;

        //string initialFileName = "OTE_" + ImportedFileFullPath.GetFileName(withoutExt: true) + "_" + DateTime.Now.ToString("MM_dd_yy_HH");

        string initialFileNameWithMoreInfo = ConcatMoreInfoToFileName("P_" + ImportedFileFullPath.GetFileName(withoutExt: true) , appendTimeNow: true);

        Func<bool> selectPassedItems = ( ) =>
        {
            bool result = Task.Run(async ( ) =>
            {
                if( m_GroupedTestCases is null ) return false;
                await m_GroupedTestCases.Where(g => g.Key == true)
                                       .SelectMany(g => g)
                                       .Select(t => t.Name)
                                       .WriteStringsToAsync(m_ExportFileFullPath);
                return true;
            }).GetAwaiter().GetResult();
            return result;
        };

        Action action = async ( ) =>
        {
            string msgBoxText = App.Current.Resources.MergedDictionaries[0]["ExportPassToLstDescription"].ToString() ?? "Export Identical excel successfully.";

            await ExportToFileAsync(m_ExportFileFullPath ,
                                    msgBoxText ,
                                    showExplorer: true ,
                                    selectPassedItems);
        };

        await ShowSaveDialog(title , filter , defaultExt , initialDirectory , initialFileNameWithMoreInfo , action);

    }


    [RelayCommand]
    public async Task ExportFailNullLst( )
    {

        _logger.Information($"({nameof(ExportFailNullLst)} Called)");

        string title = $"{App.Current.Resources.MergedDictionaries[0]["ExportFailNullDescription"]}";
        string filter = "Lst File (*.lst)|*.lst|All (*.*)|*.*";
        string defaultExt = "lst";
        string initialDirectory = LatestExportDirctory;

        //string initialFileName = ImportedFileFullPath.GetFileName(withoutExt: true) + "_" + DateTime.Now.ToString("MM_dd_yy_HH");

        string initialFileNameWithMoreInfo = ConcatMoreInfoToFileName("F_" + ImportedFileFullPath.GetFileName(withoutExt: true) , appendTimeNow: true);

        Func<bool> selectFailErrorItems = ( ) =>
        {
            bool result = Task.Run(async ( ) =>
            {
                if( m_GroupedTestCases is null ) return false;
                await m_GroupedTestCases.Where(g => g.Key != true)
                                       .SelectMany(g => g)
                                       .Select(t => t.Name)
                                       .WriteStringsToAsync(m_ExportFileFullPath);
                return true;
            }).GetAwaiter().GetResult();
            return result;
        };

        Action action = async ( ) =>
        {
            string msgBoxText = App.Current.Resources.MergedDictionaries[0]["ExportFailNullDescription"].ToString() ?? "Export Non-Identical .lst successfully.";

            await ExportToFileAsync(m_ExportFileFullPath ,
                                    msgBoxText ,
                                    showExplorer: true ,
                                    selectFailErrorItems);
        };

        await ShowSaveDialog(title , filter , defaultExt , initialDirectory , initialFileNameWithMoreInfo , action);

    }


    #region Refactoring export lst function ★☆★☆★

    public string ConcatMoreInfoToFileName(string fileNameWithoutExt , bool appendTimeNow = true)
    {
        if( TestCaseShare.Version is not null && !TestCaseShare.Version.IsNullOrWhiteSpaceOrEmpty() ) { fileNameWithoutExt += $"_{TestCaseShare.Version}"; }

        if( TestCaseShare.Media is not null && !TestCaseShare.Media.IsNullOrWhiteSpaceOrEmpty() ) { fileNameWithoutExt += $"_{TestCaseShare.Media}"; }

        if( TestCaseShare.Area is not null && !TestCaseShare.Area.IsNullOrWhiteSpaceOrEmpty() ) { fileNameWithoutExt += $"_{TestCaseShare.Area}"; }

        if( appendTimeNow ) { fileNameWithoutExt += $"_{DateTime.Now.ToString("MM_dd_yy_HH")}"; }

        return fileNameWithoutExt;
    }

    //Refactoring
    private async Task ShowSaveDialog(string title , string filter , string defaultExt , string initialDirectory , string initialFileName , Action action)
    {
        _logger.Information($"({nameof(ShowSaveDialog)} Called)");

        var settings = new SaveFileDialogSettings()
        {
            Title = title ,
            Filter = filter ,
            InitialDirectory = initialDirectory ,
            CheckPathExists = true ,
            AddExtension = true ,
            FileName = initialFileName ,
            DefaultExt = defaultExt
        };

        bool? success = _dialogService.ShowSaveFileDialog(this , settings);

        if( success == true )
        {
            m_ExportFileFullPath = settings.FileName;

            var location = $"({nameof(ShowSaveDialog)} Called).(Export File Full Path: {m_ExportFileFullPath})";

            await TasksManager.RunTaskAsync(action , location);

            //await ExportToFileAsync(m_ExportFileFullPath , m_GroupedTestCases , filterPredicate);
        }
    }

    private async Task ExportToFileAsync(string exportFileFullPath , string msgboxCustomText , bool showExplorer , Func<bool> customTask)
    {
        if( customTask is null ) { return; }

        var location = $"({nameof(ExportToFileAsync)} Called).(Export File Full Path: {exportFileFullPath})";

        bool isCustomTaskSucceed = await TasksManager.RunTaskWithReturnAsync<bool>(customTask , location , catchException: true);

        if( isCustomTaskSucceed )
        {
            var _messageBoxCaption = App.Current.Resources.MergedDictionaries[0]["SucceedExport"].ToString() ?? "Export Result";

            msgboxCustomText += Environment.NewLine
                               + Environment.NewLine
                               + exportFileFullPath
                               + Environment.NewLine
                               + App.Current.Resources.MergedDictionaries[0]["ClickYesToOpen"].ToString()
                               ?? $"Yes to open the directory of it.";

            ShowExportResultMessageBox(_messageBoxCaption , msgboxCustomText , showExplorer , exportFileFullPath);
        }
        else
        {
            var _messageBoxCaption = App.Current.Resources.MergedDictionaries[0]["FailedExport"].ToString() ?? "Export Result";

            var _messageBoxText = _messageBoxCaption
                                        + Environment.NewLine
                                        + Environment.NewLine
                                        + exportFileFullPath
                                        + Environment.NewLine;

            ShowExportResultMessageBox(_messageBoxCaption , _messageBoxText , false , exportFileFullPath);
        }
    }

    private void ShowExportResultMessageBox(string msgBoxCaption , string msgboxText , bool succeedExport , string exportFileFullPath)
    {
        MessageBoxSettings messageBoxSettings = new()
        {
            Button = succeedExport ? System.Windows.MessageBoxButton.YesNo : System.Windows.MessageBoxButton.OK ,
            Caption = msgBoxCaption ,
            Icon = succeedExport ? System.Windows.MessageBoxImage.Question : System.Windows.MessageBoxImage.Error ,
            DefaultResult = System.Windows.MessageBoxResult.Yes ,
            Options = System.Windows.MessageBoxOptions.None ,
            MessageBoxText = msgboxText ,
        };

        // Update UI Elements on the UI Thread
        App.Current.Dispatcher.Invoke(( ) =>
        {
            System.Windows.MessageBoxResult msgResult = _dialogService.ShowMessageBox(this , messageBoxSettings);

            if( !succeedExport ) { return; }

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
        });
    }


    #endregion Refactoring export lst function ★☆★☆★



    #endregion Logic

    #region UI


    [RelayCommand]
    public void ShowSelectedTestCaseRawData( )
    {
        _logger.Debug("ShowSelectedTestCaseRawDataCommand called");
        if( DiffTestCases is null ) return;
        if( CurrentSelectedIndex < 0 || DiffTestCases[CurrentSelectedIndex] is null ) return;

        _rawDataWindow.Owner = App.ViewModelLocator.Main_Window;
        WeakReferenceMessenger.Default.Send(new DiffViewer.Messages.SetRichTextBoxDocumentMessage()
        {
            Sender = this ,
            Message = "LoadRawContent" ,
            ObjReplied = DiffTestCases[CurrentSelectedIndex] ,
        });
        _rawDataWindow.Show();
    }

    [RelayCommand]
    public void SwitchLanguage(string lang)
    {
        _logger.Debug("SwitchLanguageCommand called");
        AppConfigManager.SwitchLanguageTo(lang);
    }

    [RelayCommand]
    public void ShowVSTSSettingWindow( )
    {
        _logger.Debug("ShowVSTSSettingWindow called");
        WeakReferenceMessenger.Default.Send(new DiffViewer.Messages.RefreshAccessInfoMessage()
        {
            Sender = this ,
            Message = "RefreshAccessInfos" ,
        });
        _vstsSettingWindow.Owner = App.ViewModelLocator.Main_Window;
        _vstsSettingWindow.Show();
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
    }

    /// <summary>
    /// Close Window by using WeakReferenceMessenger.
    /// </summary>
    [RelayCommand]
    public void CloseWindow( )
    {
        _logger.Debug("CloseWindowCommand called");
        App.SaveAppConfiguration();
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
