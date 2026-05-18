using System.Collections.Generic;

namespace PicoPDF.Pdf.Documents;

// https://en.wikipedia.org/wiki/Line_breaking_rules_in_East_Asian_languages
public class SimplifiedChineseLineBreakRule : ILineBreakRule
{
    public IReadOnlySet<int> DenyStartChar { get; init; } = new HashSet<int>()
    {
        '!', '%', ')', ',', '.', ':', ';', '?', ']', '}', '¢', '°', '·', '\'', '"', '†', '‡', '›', '℃', '∶', '、', '。', '〃', '〆', '〕', '〗', '〞', '﹚', '﹜', '！', '＂', '％', '＇', '）', '，', '．', '：', '；', '？', '！', '］', '｝', '～',
    };

    public IReadOnlySet<int> DenyEndChar { get; init; } = new HashSet<int>()
    {
        '$', '(', '£', '¥', '·', '\'', '"', '〈', '《', '「', '『', '【', '〔', '〖', '〝', '﹙', '﹛', '＄', '（', '．', '［', '｛', '￡', '￥',
    };
}
