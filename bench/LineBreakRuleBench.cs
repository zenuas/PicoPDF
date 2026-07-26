using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Pdf.Documents;
using Pdf.Documents.BreakRule;
using Pdf.Operation;
using System.Linq;

namespace PicoPDF.Benchmark;

public class LineBreakRuleBench
{
    [Benchmark]
    public void GenericLineBreakRule()
    {
        var rule = DrawString.GetLineBreakRule(TextStyles.LineBreak);
        var chars = new int[] { '!', ',', 'ゝ', '｝' };
        var consumer = new Consumer();

        for (var i = 0; i < 1_000_000; i++)
        {
            for (var j = 0; j < chars.Length; j++)
            {
                consumer.Consume(rule.DenyStartChar.Contains(chars[j]));
            }
        }
    }

    public static readonly int[] ArrayDenyStartChar =
        [
            ..SimplifiedChineseLineBreakRule.StaticDenyStartChar,
            ..TraditionalChineseLineBreakRule.StaticDenyStartChar,
            ..JapaneseLineBreakRule.StaticDenyStartChar,
            ..KoreanLineBreakRule.StaticDenyStartChar,
        ];

    [Benchmark]
    public void ArrayBreakRule()
    {
        var chars = new int[] { '!', ',', 'ゝ', '｝' };
        var consumer = new Consumer();

        for (var i = 0; i < 1_000_000; i++)
        {
            for (var j = 0; j < chars.Length; j++)
            {
                consumer.Consume(ArrayDenyStartChar.Contains(chars[j]));
            }
        }
    }
}
