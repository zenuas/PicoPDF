namespace PicoPDF.Pdf.Documents;

public class NoneLineBreakRule : ILineBreakRule
{
    public int[] DenyStartChar { get; init; } = [];
    public int[] DenyEndChar { get; init; } = [];
}
