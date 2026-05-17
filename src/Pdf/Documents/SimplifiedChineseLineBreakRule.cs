namespace PicoPDF.Pdf.Documents;

// https://en.wikipedia.org/wiki/Line_breaking_rules_in_East_Asian_languages
public class SimplifiedChineseLineBreakRule : ILineBreakRule
{
    public int[] DenyStartChar { get; init; } = [
            '!', '%', ')', ',', '.', ':', ';', '?', ']', '}', '¢', '°', '·', '\'', '"', '†', '‡', '›', '℃', '∶', '、', '。', '〃', '〆', '〕', '〗', '〞', '﹚', '﹜', '！', '＂', '％', '＇', '）', '，', '．', '：', '；', '？', '！', '］', '｝', '～',
        ];

    public int[] DenyEndChar { get; init; } = [
            '$', '(', '£', '¥', '·', '\'', '"', '〈', '《', '「', '『', '【', '〔', '〖', '〝', '﹙', '﹛', '＄', '（', '．', '［', '｛', '￡', '￥',
        ];
}
