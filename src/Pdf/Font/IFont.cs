namespace PicoPDF.Pdf.Font;

public interface IFont : IPdfObject
{
    public string Name { get; init; }
    public string CreateTextShowingOperator(string s);
}
