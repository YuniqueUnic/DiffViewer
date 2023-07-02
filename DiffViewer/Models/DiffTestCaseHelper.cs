using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiffViewer.Models;

public static class DiffTestCaseHelper
{
    public const string sIdentical = "are identical";
    public const string sSevereError1 = "No such file or directory";
    public const string sSevereError2 = "SEVERE ERROR";
    public const string sSevereError3 = "No dump file";
    public const string sPostProcessingString = "Post Processing...";

    static void IsNullTestCase(DiffTestCase testCase)
    {
        if( testCase is null )
        {
            App.Logger.Error($"{testCase} is null");
            throw new NullReferenceException($"{testCase} is null");
        }
    }

    /// <summary>
    /// Set the test case's Raw property
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="raw"></param>
    public static DiffTestCase SetRaw(this DiffTestCase testCase , string raw)
    {
        IsNullTestCase(testCase);
        testCase.Raw = raw;
        testCase.RawSize = raw.Length;
        return testCase;
    }

    /// <summary>
    /// Set the test case's RawSize property
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="raw"></param>
    public static DiffTestCase SetRawSize(this DiffTestCase testCase , string raw)
    {
        IsNullTestCase(testCase);

        // Calculate the size of the UTF-8 encoded byte array
        byte[] bytes = Encoding.UTF8.GetBytes(raw);
        int size = bytes.Length;
        // Add the length of the newline character
        size += Environment.NewLine.Length;
        // Add the length of the notepad file header: BOM (Byte Order Mark) and the UTF-8 encoding
        size += 4;
        double sizeInKB = Math.Round(size / 1024.0 , 2);
        testCase.RawSize = sizeInKB;
        return testCase;
    }

    /// <summary>
    /// Set the test case's NewText_Actual property
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="newText"></param>
    public static DiffTestCase SetNewText(this DiffTestCase testCase , string newText)
    {
        IsNullTestCase(testCase);
        testCase.NewText_Actual = newText;
        return testCase;
    }

    /// <summary>
    /// Set the test case's OldText_BaseLine property
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="oldText"></param>
    public static DiffTestCase SetOldText(this DiffTestCase testCase , string oldText)
    {
        IsNullTestCase(testCase);
        testCase.OldText_BaseLine = oldText;
        return testCase;
    }

    /// <summary>
    /// Set the test case's IsIdentical property
    /// If the text ends with "are identical", the test case is Identical   = true
    /// If not, the test case is not identical,                 IsIdentical = false
    /// If null, the test case is a severe error,               IsIdentical = null
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="identicalText"></param>
    public static DiffTestCase SetIdentical(this DiffTestCase testCase , string identicalText)
    {
        IsNullTestCase(testCase);
        if( identicalText.EndsWith(sIdentical) || identicalText.Contains(sIdentical) )
        {
            testCase.IsIdentical = true;
        }
        else if( identicalText.Contains(sSevereError1) || identicalText.Contains(sSevereError2) || identicalText.Contains(sSevereError3) )
        {
            testCase.IsIdentical = null;
        }
        else
        {
            testCase.IsIdentical = false;
        }
        return testCase;
    }

    /// <summary>
    /// Set the test case's MoreInfo property
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="testCaseShare"></param>
    public static void SetMoreInfo(this DiffTestCase testCase , TestCaseShare testCaseShare)
    {
        testCase.MoreInfo = testCaseShare;
    }

    /// <summary>
    /// Set the test case's MoreInfo property with the same value
    /// </summary>
    /// <param name="list"></param>
    /// <param name="testCaseShare"></param>
    public static void SetMoreInfo(this List<DiffTestCase> list , TestCaseShare testCaseShare)
    {
        Parallel.ForEach(list , (testCase) =>
        {
            testCase.MoreInfo = testCaseShare;
        });
    }

    /// <summary>
    /// Get the version number from the test case
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>    
    public static int GetVersionTested(this DiffTestCase testCase)
    {
        IsNullTestCase(testCase);
        return int.TryParse(testCase.MoreInfo?.Version , out var version) ? version : -1;
    }

    /// <summary>
    /// Get the media number from the test case
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>
    public static int GetMediaTested(this DiffTestCase testCase)
    {
        IsNullTestCase(testCase);
        return int.TryParse(testCase.MoreInfo?.Media , out var media) ? media : -1;
    }

    /// <summary>
    /// Get the date tested from the test case
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>
    public static DateTime GetDateTested(this DiffTestCase testCase)
    {
        IsNullTestCase(testCase);
        return DateTime.TryParse(testCase.MoreInfo?.Time , out var dateTested) ? dateTested : DateTime.MinValue;


    }

    /// <summary>
    /// Create a List<TestCase> only with the name thourgh the List<string>
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<DiffTestCase> CreateTCswithName(this List<string> list)
    {
        List<DiffTestCase> tcList = new List<DiffTestCase>();
        for( int i = 0; i < list.Count; i++ )
        {
            tcList.Add(new DiffTestCase() { Name = list[i] });
        }
        return tcList;
    }
}
