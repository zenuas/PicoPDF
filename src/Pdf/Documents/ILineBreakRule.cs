namespace PicoPDF.Pdf.Documents;

public interface ILineBreakRule
{
    public int[] DenyStartChar { get; init; }
    public int[] DenyEndChar { get; init; }
}
