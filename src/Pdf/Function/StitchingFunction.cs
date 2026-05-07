namespace PicoPDF.Pdf.Function;

public class StitchingFunction : PdfObject, IFunction
{
    public FunctionTypes FunctionType { get; init; } = FunctionTypes.StitchingFunction;
    public required float[] Domain { get; init; }
    public required IFunction[] Functions { get; init; }
    public required float[] Bounds { get; init; }
    public required float[] Encode { get; init; }
}
