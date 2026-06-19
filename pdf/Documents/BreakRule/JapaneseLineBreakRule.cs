using System.Collections.Generic;

namespace Pdf.Documents.BreakRule;

// JIS X 4051
public class JapaneseLineBreakRule : ILineBreakRule
{
    public IReadOnlySet<int> DenyStartChar { get; init; } = new HashSet<int>()
    {
        ',', ')', ']', '｝', '、', '〕', '〉', '》', '」', '』', '】', '〙', '〗', '〟', '’', '”', '｠', '»',
        'ゝ', 'ゞ', 'ー',
        'ァ', 'ィ', 'ゥ', 'ェ', 'ォ',
        'ッ', 'ャ', 'ュ', 'ョ', 'ヮ', 'ヵ', 'ヶ',
        'ぁ', 'ぃ', 'ぅ', 'ぇ', 'ぉ',
        'っ', 'ゃ', 'ゅ', 'ょ', 'ゎ', 'ゕ', 'ゖ',
        'ㇰ', 'ㇱ', 'ㇲ', 'ㇳ', 'ㇴ',
        'ㇵ', 'ㇶ', 'ㇷ', 'ㇸ', 'ㇹ',
        'ㇺ',
        'ㇻ', 'ㇼ', 'ㇽ', 'ㇾ', 'ㇿ',
        '々', '〻',
        '?', '!', '‼', '⁇', '⁈', '⁉',
        '・', ':', ';', '/',
        '。', '.',
    };

    public IReadOnlySet<int> DenyEndChar { get; init; } = new HashSet<int>()
    {
        '(', '[', '｛', '〔', '〈', '《', '「', '『', '【', '〘', '〖', '〝', '‘', '“', '｟', '«',
    };
}
