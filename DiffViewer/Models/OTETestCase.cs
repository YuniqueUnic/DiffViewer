using System.Collections.Generic;
using System.Linq;

namespace DiffViewer.Models;

public interface ITestCase
{
    public void SetIndex(int index);
    public int GetIndex( );
    public void SetScriptName(string scriptName);
    public string? GetScriptName( );
}


public class ScriptInfo
{
    public string? ScriptName { get; set; }
    public string? ScriptArea { get; set; }
}

public class ScriptTestCasesMatchResult
{
    public bool AllMatch { get; set; }
    public IEnumerable<ScriptInfo>? ScriptsMatched { get { return TestCasesMatched?.Select(x => new ScriptInfo { ScriptName = x.GetScriptName() }).Distinct(); } }
    public IEnumerable<ScriptInfo>? ScriptsNotMatched { get; set; }
    public IEnumerable<ITestCase>? TestCasesMatched { get; set; }
    public IEnumerable<ITestCase>? TestCasesNotMatched { get; set; }
}

public class OTETestCase : ITestCase
{
    private int Index { get; set; } = -1;
    public int TestCaseId { get; set; } = -1;
    public string? Title { get; set; }
    public string? TestStep { get; set; } = string.Empty;
    public string? StepAction { get; set; } = string.Empty;
    public string? StepExpected { get; set; } = string.Empty;
    public int TestPointId { get; set; } = -1;
    public string? Configuration { get; set; }
    public string? AssignedTo { get; set; }
    public string? Outcome { get; set; }
    public string? Comment { get; set; } = string.Empty;
    public string? Defects { get; set; } = string.Empty;
    public string? RunBy { get; set; }
    private string? ScriptName { get; set; }

    public void SetIndex(int index) => Index = index;
    public int GetIndex( ) => Index;
    public void SetScriptName(string scriptName) => ScriptName = scriptName;
    public string? GetScriptName( ) => ScriptName;
}

public class OTEDetailTestCase : ITestCase
{
    private int Index { get; set; } = -1;
    public int TestCaseId { get; set; } = -1;
    public string? Title { get; set; }
    public string? TestStep { get; set; } = string.Empty;
    public string? StepAction { get; set; } = string.Empty;
    public string? StepExpected { get; set; } = string.Empty;
    public int TestPointId { get; set; } = -1;
    public string? Configuration { get; set; }
    public string? AssignedTo { get; set; }
    public string? Outcome { get; set; }
    public string? Comment { get; set; } = string.Empty;
    public string? Defects { get; set; } = string.Empty;
    public string? RunBy { get; set; }
    public string? ScriptName { get; set; }
    public double RawDataSize { get; set; } = -1;
    public string SizeUnit { get; private set; } = "KB";

    public void SetScriptName(string scriptName) => ScriptName = scriptName;
    public string? GetScriptName( ) => ScriptName;
    public void SetIndex(int index) => Index = index;
    public int GetIndex( ) => Index;
}

