namespace DiffViewer.Models;

public class TestCase
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
