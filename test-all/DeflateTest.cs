using Mina.Extension;
using System;
using System.IO;
using System.IO.Compression;

namespace PicoPDF.TestAll;

public class DeflateTest
{
    public static void Deflate(Option opt)
    {
        using var input = new FileStream(opt.InputDeflate, FileMode.Open);
        using var output = new FileStream(opt.Output is { } o && o != "" ? o : $"{opt.InputDeflate}.deflate", FileMode.Create);
        Console.Write($"{input.ReadByte():x2} ");
        Console.Write($"{input.ReadByte():x2} ");
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        deflate.EnumerableReadBytes().Each(output.WriteByte);

    }
}
