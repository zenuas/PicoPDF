using Mina.Command;
using Mina.Extension;
using System;
using System.IO;
using System.IO.Compression;

namespace PicoPDF.TestAll;

public class Deflate : ICommand
{
    [CommandOption("input"), CommandOption('i')]
    public string Input { get; init; } = "";

    [CommandOption("output"), CommandOption('o')]
    public string Output { get; init; } = "";

    public void Run(string[] args)
    {
        using var input = new FileStream(Input, FileMode.Open);
        using var output = new FileStream(Output != "" ? Output : $"{Input}.deflate", FileMode.Create);
        Console.Write($"{input.ReadByte():x2} ");
        Console.Write($"{input.ReadByte():x2} ");
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        deflate.EnumerableReadBytes().Each(output.WriteByte);
    }
}
