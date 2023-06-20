using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffViewer.Models;

internal class OpenSourceProject
{
    public string? Name { get; set; } = string.Empty;
    public string? Version { get; set; } = string.Empty;
    public string? License { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
}