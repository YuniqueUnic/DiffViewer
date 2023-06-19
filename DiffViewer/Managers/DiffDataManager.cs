using DiffViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffViewer.Managers;

public class DiffDataManager
{
    #region Fields
    const char m_SplitChar = '#';
    const char m_LineChar = '@';
    const char m_UnitOpChar = '^';
    const char m_PlusChar = '+';
    const char m_MinusChar = '-';

    const string m_Completedin = " completed in ";
    const string m_Identical = "are identical";
    const string m_PostProcessing = "Post Processing...";

    const string m_ValidationResults = "Validation Results";
    const string m_EndofSummary = "End of Summary";
    private bool m_IsContainValidation;
    private bool m_IsContainEndofSummary;

    private string _diffFilePath;
    private string? _diffNames;
    private List<string> _diffResults = new();

    #endregion Fields



    #region Properties

    /// <summary>
    /// Get Diff Text LineCount and full Content by using FileManager.GetTextInfoAsync().
    /// </summary>
    public (int, string?) DiffInfos => FileManager.GetTextInfoAsync(_diffFilePath).Result;

#if DEBUG

    /// <summary>
    /// Get Diff TestCase Names.
    /// </summary>
    public List<string>? DiffNames
    {
        get; private set;
    }

    /// <summary>
    /// Get all Diff Compare Results.
    /// </summary>
    public List<string>? DiffResults
    {
        get { return _diffResults; }
    }

#endif 


    public List<TestCase>? TestCases { get; private set; }

    /// <summary>
    /// Check whether the Diff Process is over.
    /// If over, return true.
    /// not over, return false.
    /// </summary>
    public bool IsProcessOver { get; private set; }

    #endregion Properties


    /// <summary>
    /// Init DiffDataManager with diffFilePath.
    /// </summary>
    /// <param name="diffFilePath"></param>
    public DiffDataManager(string diffFilePath)
    {
        FileManager.CheckFileExists(diffFilePath , true);
        _diffFilePath = diffFilePath;
    }

    /// <summary>
    /// Handle diff file to get the Diff Compare Results and Diff TestCase Names.
    /// </summary>
    /// <returns></returns>
    public async Task<DiffDataManager> HandleDiff( )
    {
        await LoadAndProcessDiffFileAsync(_diffFilePath);
#if DEBUG
        DiffNames = await ExtractTestCasesNameAsync(_diffNames);
        TestCases = DiffNames.CreateTCswithName();
        TestCases = await ExtractTestCaseDiffResultAsync(_diffResults , TestCases);
#endif

#if !DEBUG
        TestCases = (await ExtractTestCasesNameAsync(_diffNames)).CreateTCswithName();
        TestCases = await ExtractTestCaseDiffResultAsync(_diffResults , TestCases);
#endif
        return this;
    }

    /// <summary>
    /// Load large Txt file by using FileStream and StreamReader.
    /// And using StringBuilder to concat strings.
    /// </summary>
    /// <param name="diffFilePath"></param>
    private async Task LoadAndProcessDiffFileAsync(string diffFilePath)
    {
        App.Logger.Information($"Start handling Diff File: {diffFilePath}");
        string? location = $"{nameof(DiffDataManager)}.{nameof(HandleDiff)}";

        await TasksManager.RunTaskAsync(async ( ) =>
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int bufferSize = 4096;
                int numPostProcessing = 0;
                using( FileStream fs = new FileStream(diffFilePath , FileMode.Open , FileAccess.Read , FileShare.Read , bufferSize , useAsync: true) )
                {
                    using( StreamReader sr = new StreamReader(fs , Encoding.UTF8 , true , bufferSize) )
                    {
                        while( !sr.EndOfStream )
                        {
                            string line = sr.ReadLine();
                            sb.AppendLine(line);

                            // Get the TestCases Name Content.
                            if( !m_IsContainEndofSummary )
                            {
                                if( !m_IsContainValidation )
                                {
                                    if( line.Contains(m_ValidationResults) ) { m_IsContainValidation = true; }
                                }

                                if( line.Contains(m_EndofSummary) )
                                {
                                    if( !m_IsContainValidation ) throw new Exception($"The {m_ValidationResults} have not been found! File Struct is not right!!!");

                                    _diffNames = sb.ToString();
                                    App.Logger.Information($"The {m_ValidationResults} have been found! Diff Name Content got.");
                                    m_IsContainEndofSummary = true;
                                    // Clear Content of StringBuilder.
                                    sb.Clear();
                                }
                            }

                            // Get the Diff Content for each TestCase.
                            if( m_IsContainEndofSummary && m_IsContainValidation )
                            {
                                if( numPostProcessing == 1 && (line.StartsWith(m_PostProcessing) || line.Contains(m_PostProcessing)) )
                                {
                                    sb.Remove(sb.Length - line.Length - 2 , line.Length);
                                    _diffResults.Add(sb.ToString());
                                    sb.Clear();
                                    sb.AppendLine(line);
                                }
                                else if( numPostProcessing == 0 && (line.StartsWith(m_PostProcessing) || line.Contains(m_PostProcessing)) )
                                {
                                    sb.Clear();
                                    sb.AppendLine(line);
                                    numPostProcessing = 1;
                                }
                            }
                        }
                    }
                }

                //Add the last one TestCase records.
                _diffResults.Add(sb.ToString());

                IsProcessOver = true;
                App.Logger.Information($"End of handling Diff File: {diffFilePath}");
            }
            catch( System.Exception )
            {
                App.Logger.Error($"{nameof(DiffDataManager)} LoadAndProcessDiffFile failed!");
                throw;
            }
        } , location);

    }

    /// <summary>
    /// Extract the TestCases Name from the Diff Names String.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<List<string>> ExtractTestCasesNameAsync(string diffNamesString)
    {
        string? location = $"{nameof(DiffDataManager)}.{nameof(ExtractTestCasesNameAsync)}";

        return await TasksManager.RunTaskWithReturnAsync(( ) =>
        {
            CheckProcessOver();
            List<string> testCaselist = new List<string>();
            string[] strings = diffNamesString?.Split('\n' , StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? throw new Exception();

            for( int i = 0; i < strings.Length; i++ )
            {
                if( strings[i].Contains(m_Completedin) )
                {
                    testCaselist.Add(strings[i].Substring(0 , strings[i].IndexOf(m_Completedin , StringComparison.OrdinalIgnoreCase)));
                }
            }

            return testCaselist;

        } , location , catchException: true);

    }

    /// <summary>
    /// Extract the Diff Compare Result for each TestCase.
    /// Update the TestCase Diff Result.
    /// Name, Raw, OldText, NewText, IsIdentical
    /// </summary>
    /// <param name="diffTCResultList"></param>
    /// <param name="testCases"></param>
    /// <returns></returns>
    private async Task<List<TestCase>> ExtractTestCaseDiffResultAsync(List<string> diffTCResultList , List<TestCase> testCases)
    {
        CheckProcessOver();
        var location = $"{nameof(DiffDataManager)}.{nameof(ExtractTestCaseDiffResultAsync)}";

        return await TasksManager.RunTaskWithReturnAsync(( ) =>
        {
            List<TestCase> m_testCases = testCases;

            diffTCResultList.ForEach((s) =>
            {
                string[] strings = s.Split('\n' , StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                string? name = null;

                StringBuilder sb = new StringBuilder();

                for( int i = 0; i < strings.Length; i++ )
                {
                    if( strings[i].StartsWith(m_SplitChar) && strings[i].EndsWith(m_SplitChar) )
                    {
                        continue;
                    }
                    else if( name is null && (strings[i].StartsWith(m_PostProcessing) || strings[i].Contains(m_PostProcessing)) )
                    {
                        name = strings[i].Replace(m_PostProcessing , string.Empty).Trim();
                        continue;
                    }
                    sb.AppendLine(strings[i]);
                }

                var raw = sb.ToString();

                var location = $"{nameof(DiffDataManager)}.{nameof(ExtractTestCaseDiffResultAsync)}.{nameof(SplitResult)}";
                var splitedData = TasksManager.RunTaskWithReturn(( ) =>
                {
                    return SplitResult(raw);
                } , location , catchException: true , throwException: false);

                try
                {

                    // Update the TestCase some major Infos.
                    m_testCases.First(t => t.Name.Contains(name) || t.Name.Equals(name))
                               .SetRaw(raw)
                               .SetNewText(splitedData.actualText)
                               .SetOldText(splitedData.baseLineText)
                               .SetIdentical(raw);
                }
                catch( Exception ex )
                {
                    App.Logger.Error($"Current handling TestCase: {name}" +
                                     $"{Environment.NewLine}Exception: {ex.Message}");
                    //throw;
                }

            });
            return m_testCases;
        } , location , catchException: true , throwException: false);

    }

    /// <summary>
    /// Split the diff Result to Baseline and Actual
    /// OldText should start with + m_PlusChar
    /// NewText should start with - m_MinusChar
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private (string baseLineText, string actualText) SplitResult(string input)
    {
        // Split content
        var lines = input.Split('\n').ToList();

        // Find the Index of separator 
        var plusIndices = new List<int>();
        var minusIndices = new List<int>();
        for( int i = 0; i < lines.Count; i++ )
        {
            if( lines[i].StartsWith(m_PlusChar) )
            {
                plusIndices.Add(i);
            }
            else if( lines[i].StartsWith(m_MinusChar) )
            {
                minusIndices.Add(i);
            }

        }


        // Create two string lists based on the index
        // OldText_baseLine should start with + m_PlusChar
        // NewText_actual   should start with - m_MinusChar
        var baseLineLines = new List<string>(lines);
        var actualLines = new List<string>(lines);

        // Delete lines beginning with + sign in actualLines to leave the content start with - sign
        for( int i = 0; i < plusIndices.Count; i++ )
        {
            if( actualLines[plusIndices[i] - i].StartsWith(m_PlusChar) )
            {
                actualLines.RemoveAt(plusIndices[i] - i);
            }
        }

        // Delete lines beginning with - sign in baseLineLines to leave the content start with + sign
        for( int i = 0; i < minusIndices.Count; i++ )
        {
            if( baseLineLines[minusIndices[i] - i].StartsWith(m_MinusChar) )
            {
                baseLineLines.RemoveAt(minusIndices[i] - i);
            }
        }

        // Concat list and return
        return (string.Join("\n" , baseLineLines), string.Join("\n" , actualLines));
    }

    /// <summary>
    /// Check if the Diff Data handled over.
    /// </summary>
    /// <param name="throwException"> true for throwing an error if IsProcessOver is false </param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>

    public bool CheckProcessOver(bool throwException = true)
    {
        if( !IsProcessOver )
        {
            App.Logger.Error($"{nameof(DiffDataManager)}Haven't load and process the diff data first!!!");
            if( throwException )
            {
                throw new NotSupportedException("Haven't load and process the diff data first!!!");
            }
            return false;
        }
        return true;
    }

}