using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PicoPDF.Pdf.Documents;
using System.Linq;

namespace PicoPDF.Benchmark;

public class LineBreakRuleBench
{
    [Benchmark]
    public void GenericLineBreakRule()
    {
        var rule = new GenericLineBreakRule();
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
            ..new SimplifiedChineseLineBreakRule().DenyStartChar,
            ..new TraditionalChineseLineBreakRule().DenyStartChar,
            ..new JapaneseLineBreakRule().DenyStartChar,
            ..new KoreanLineBreakRule().DenyStartChar,
        ];

    [Benchmark]
    public void ArrayBreakRule()
    {
        var rule = new GenericLineBreakRule();
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
