using BenchmarkDotNet.Attributes;
using PicoPDF.OpenType.Tables.PostScript;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.Benchmark;

public class WriteIndexDataBench
{
    public static IEnumerable<byte> MakeBytes(int count)
    {
        byte b = 0;
        for (var i = 0; i < count; i++)
        {
            yield return b++;
        }
    }

    [Benchmark]
    public void WriteIndexData1()
    {
        var data1 = MakeBytes(254).ToArray();
        byte[][] bytes = [[], data1];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }

    [Benchmark]
    public void WriteIndexData2()
    {
        var data1 = MakeBytes(255).ToArray();
        var data2 = MakeBytes(65279).ToArray();
        byte[][] bytes = [[], data1, data2];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }

    [Benchmark]
    public void WriteIndexData3()
    {
        var data1 = MakeBytes(255).ToArray();
        var data2 = MakeBytes(65280).ToArray();
        var data3 = MakeBytes(16711679).ToArray();
        byte[][] bytes = [[], data1, data2, data3];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }

    [Benchmark]
    public void WriteIndexData4()
    {
        var data1 = MakeBytes(255).ToArray();
        var data2 = MakeBytes(65280).ToArray();
        var data3 = MakeBytes(16711680).ToArray();
        byte[][] bytes = [[], data1, data2, data3];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }
}
