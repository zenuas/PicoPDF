using BenchmarkDotNet.Attributes;
using PicoPDF.OpenType.Tables.PostScript;
using System.Collections.Generic;
using System.IO;

namespace PicoPDF.Benchmark;

public class WriteIndexDataBench
{
    public byte[] bytes254_ = [.. MakeBytes(254)];
    public byte[] bytes255_ = [.. MakeBytes(255)];
    public byte[] bytes65279_ = [.. MakeBytes(65279)];
    public byte[] bytes65280_ = [.. MakeBytes(65280)];
    public byte[] bytes16711679_ = [.. MakeBytes(16711679)];
    public byte[] bytes16711680_ = [.. MakeBytes(16711680)];

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
        byte[][] bytes = [[], bytes254_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }

    [Benchmark]
    public void WriteIndexData2()
    {
        byte[][] bytes = [[], bytes255_, bytes65279_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }

    [Benchmark]
    public void WriteIndexData3()
    {
        byte[][] bytes = [[], bytes255_, bytes65280_, bytes16711679_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }

    [Benchmark]
    public void WriteIndexData4()
    {
        byte[][] bytes = [[], bytes255_, bytes65280_, bytes16711680_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
    }
}
