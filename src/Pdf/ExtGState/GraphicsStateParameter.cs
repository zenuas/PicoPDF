namespace PicoPDF.Pdf.ExtGState;

public class GraphicsStateParameter : PdfObject, IGraphicsStateParameter
{
    public required string Name { get; init; }
}
