using System.Collections.Generic;

namespace Pdf.Documents;

public class GenericLineBreakRule : ILineBreakRule
{
    public IReadOnlySet<int> DenyStartChar { get; init; } = new HashSet<int>(
        [
            ..new SimplifiedChineseLineBreakRule().DenyStartChar,
            ..new TraditionalChineseLineBreakRule().DenyStartChar,
            ..new JapaneseLineBreakRule().DenyStartChar,
            ..new KoreanLineBreakRule().DenyStartChar,
        ]);

    public IReadOnlySet<int> DenyEndChar { get; init; } = new HashSet<int>(
        [
            ..new SimplifiedChineseLineBreakRule().DenyEndChar,
            ..new TraditionalChineseLineBreakRule().DenyEndChar,
            ..new JapaneseLineBreakRule().DenyEndChar,
            ..new KoreanLineBreakRule().DenyEndChar,
        ]);
}
