using System.Collections.Generic;

namespace Pdf.Documents.BreakRule;

// https://en.wikipedia.org/wiki/Line_breaking_rules_in_East_Asian_languages
public class KoreanLineBreakRule : ILineBreakRule
{
    public static readonly IReadOnlySet<int> StaticDenyStartChar = new HashSet<int>()
    {
        '!', '%', ')', ',', '.', ':', ';', '?', ']', '}', '¢', '°', '\'', '"', '†', '‡', '℃', '〆', '〈', '《', '「', '『', '〕', '！', '％', '）', '，', '．', '：', '；', '？', '］', '｝',
    };
    public static readonly IReadOnlySet<int> StaticDenyEndChar = new HashSet<int>()
    {
        '$', '(', '[', '\\', '{', '£', '¥', '\'', '"', '々', '〇', '〉', '》', '」', '〔', '＄', '（', '［', '｛', '｠', '￥', '￦', '#',
    };

    public IReadOnlySet<int> DenyStartChar { get; init; } = StaticDenyStartChar;
    public IReadOnlySet<int> DenyEndChar { get; init; } = StaticDenyEndChar;
}
