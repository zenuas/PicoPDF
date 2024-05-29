using PicoPDF.Sample;
using System;
using System.IO;
using System.Linq;

var csv = File.ReadAllLines("test-case/test.csv")
    .Skip(1)
    .Select(x => x.Split(','))
    .Select(x => new Data(x[0], x[1], x[2], int.Parse(x[3])))
    .ToArray();

var doc = new PicoPDF.Pdf.Document();
doc.FontRegister.RegistDirectory([@"test-case", Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts")]);
var pagesection = PicoPDF.Binder.JsonLoader.Load("test-case/test-01.json");
var pages = PicoPDF.Binder.SectionBinder.Bind(pagesection, csv);
PicoPDF.Model.ModelMapping.Mapping(doc, pages);
doc.Save("@a.pdf", new() { ContentsStreamDeflate = false, Debug = true });

//using var input = new FileStream("sample_in.txt", FileMode.Open);
//using var output = new FileStream("sample_out.txt", FileMode.Create);
//Console.Write($"{input.ReadByte():x2} ");
//Console.Write($"{input.ReadByte():x2} ");
//using var deflate = new DeflateStream(input, CompressionMode.Decompress);
//deflate.EnumerableReadBytes().Each(output.WriteByte);
