using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PDF_CREATE;

public interface IFileGenerator
{
    byte[] GeneratePdf(Action<IDocumentContainer> design);
}

public class QuestPdfGenerator : IFileGenerator
{
    public byte[] GeneratePdf(Action<IDocumentContainer> design)
    {
        var doc = Document.Create(design);
        return doc.GeneratePdf();
    }
}