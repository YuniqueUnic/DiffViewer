namespace DiffViewer.Models;

class RequestMsg
{
    public object? Sender { get; set; }
    public string? Message { get; set; }
    public bool BoolReplied { get; set; }
    public object? ObjReplied { get; set; }
}
