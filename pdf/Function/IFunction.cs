namespace Pdf.Function;

public interface IFunction
{
    public FunctionTypes FunctionType { get; init; }
    public float[] Domain { get; init; }
}
