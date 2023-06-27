using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffViewer.Models;

public class OTETestCase
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
    // Just for Diff Viewer
    private string? ScriptName { get; set; }

    public void SetIndex(int index) => Index = index;
    public int GetIndex() => Index;
    public void SetScriptName(string scriptName) => ScriptName = scriptName;
    public string? GetScriptName() => ScriptName;
}