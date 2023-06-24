using System.Windows.Documents;

namespace DiffViewer.Managers;

public static class FlowDocumentManager
{
    public static FlowDocument CreateFlowDocument(string text)
    {
        FlowDocument document = new FlowDocument();
        string[] paragraphs = text.Split('\n');
        foreach( string paragraph in paragraphs )
        {
            Paragraph newParagraph = new Paragraph();
            string[] lines = paragraph.Split('\r');
            foreach( string line in lines )
            {
                newParagraph.Inlines.Add(new Run(line));
            }
            document.Blocks.Add(newParagraph);
        }
        return document;
    }
}
