namespace Pdf.Font;

public interface IFont
{
    public string Name { get; init; }
    public string CreateTextShowingOperator(string s);
}
