namespace DiffViewer.Models;

public class DiffTestCase
{
    public string? Name { get; set; }
    /// <summary>
    /// Identical     = true   ---> No Color
    /// Not Identical = false  ---> Red Color
    /// Severe Error  = null   ---> Red Color + Bold
    /// </summary>
    public bool? IsIdentical { get; set; }
    public string? Raw { get; set; }
    public string? OldText_BaseLine { get; set; }
    public string? NewText_Actual { get; set; }
    public TestCaseShare? MoreInfo { get; set; }
}

public class TestCaseShare
{
    public string? Area { get; set; }
    public string? Media { get; set; }
    public string? Version { get; set; }
    public string? Time { get; set; }
}


public class OTETestCase
{
    public int Index { get; set; } = -1;
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
    public string? ScriptName { get; set; }
}