using Extensions;
using PicoPDF.Document;
using PicoPDF.Document.Color;
using PicoPDF.Document.Font;
using PicoPDF.Section.Element;
using System.IO.Compression;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var doc = new Document();
var stdtype1 = doc.AddFont("stdtype1", StandardType1Fonts.TimesRoman, Encoding.GetEncoding(932));
var type1 = doc.AddFont("type1", "Ryumin-Light", "WinAnsiEncoding", Encoding.GetEncoding(932));
var cif_sjis = doc.AddFont("cif_sjis", "HeiseiMin-W3", CMap._90msp_RKSJ_H, Encoding.GetEncoding(932));
var cif_utf16 = doc.AddFont("cif_utf16", "MS-Gothic", CMap.UniJIS_UTF16_H, Encoding.BigEndianUnicode);
var gray = new DeviceGray() { Gray = 0.5 };
var red = new DeviceRGB() { R = 1.0, G = 0.0, B = 0.0 };
var cyan = new DeviceCMYK() { C = 1.0, M = 0.0, Y = 0.0, K = 0.0 };

var page1 = doc.NewPage(PageSize.A4, Orientation.Horizontal).Contents;
var size = 30;
page1.DrawString($"Hello World(StandardType1/{stdtype1.Font.GetAttributeOrDefault<FontNameAttribute>()!.Name})", 50, 100, size, stdtype1, cyan);
page1.DrawString($"Hello World(Type1/{type1.BaseFont})", 50, 200, size, type1, red);
page1.DrawString($"ハローワールド(CIF/Type0/{cif_sjis.BaseFont})", 50, 300, size, cif_sjis);
page1.DrawString($"ハローワールド(CIF/Type0/{cif_utf16.BaseFont})", 50, 400, size, cif_utf16);
page1.DrawString($"{DateTime.Now:G}", 50, 500, size, type1, gray);
page1.DrawLine(50, 400, 50 + (size * 40 / 2), 400, red);
page1.DrawRectangle(50, 400 - size + 5, size * 40 / 2, size, cyan);
page1.DrawLine(50, 502, 50 + (size * 10 / 2), 502, cyan);
page1.DrawLine(50, 502, 50, 502 - size);
page1.DrawRectangle(50, 505, size * 10 / 2, 10, red);
page1.DrawFillRectangle(50, 525, size * 10 / 2, 10, red, cyan);

var page2 = doc.NewPage(PageSize.A4).Contents;
page2.DrawString("Hello World2", 100, 50, 20, cif_sjis);
doc.Save("a.pdf");

var p = new BindElement() { X = 12345, Y = 2, Bind = "a", Format = "b" };
var pf = Expressions.GetProperty<BindElement, object>("X");
Console.WriteLine(pf(p).Cast<IFormattable>().ToString("#,0", null));

//using var input = new FileStream("sample_in.txt", FileMode.Open);
//using var output = new FileStream("sample_out.txt", FileMode.Create);
//Console.Write($"{input.ReadByte():x2} ");
//Console.Write($"{input.ReadByte():x2} ");
//using var deflate = new DeflateStream(input, CompressionMode.Decompress);
//deflate.ReadAllBytes().Each(output.WriteByte);

var fontdir = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts");
var fonts = Directory.GetFiles(fontdir, "*.*", SearchOption.AllDirectories)
    .Where(x => Path.GetExtension(x).In(".TTF", ".TTC"))
    .Select(x => Path.GetExtension(x) == ".TTF" ? [TrueTypeFont.Load(x)] : TrueTypeFont.LoadCollection(x))
    .Flatten()
    .ToArray();

fonts.Each(x => Console.WriteLine(x.Name));
