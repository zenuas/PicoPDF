namespace PicoPDF.Pdf.Documents;

public class GenericLineBreakRule : ILineBreakRule
{
    public int[] DenyStartChar { get; init; } = [
            .. new SimplifiedChineseLineBreakRule().DenyStartChar,
            .. new TraditionalChineseLineBreakRule().DenyStartChar,
            .. new JapaneseLineBreakRule().DenyStartChar,
            .. new KoreanLineBreakRule().DenyStartChar,
        ];

    public int[] DenyEndChar { get; init; } = [
            .. new SimplifiedChineseLineBreakRule().DenyEndChar,
            .. new TraditionalChineseLineBreakRule().DenyEndChar,
            .. new JapaneseLineBreakRule().DenyEndChar,
            .. new KoreanLineBreakRule().DenyEndChar,
        ];
}
