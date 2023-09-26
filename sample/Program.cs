using Extensions;
using PicoPDF.Image;
using PicoPDF.Sample;

var csv = File.ReadAllLines("test-case/test.csv")
    .Skip(1)
    .Select(x => x.Split(','))
    .Select(x => new Data(x[0], x[1], x[2], int.Parse(x[3])))
    .ToArray();

var doc = new PicoPDF.Pdf.Document();
doc.FontRegister.RegistDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts"));
var pagesection = PicoPDF.Binder.JsonLoader.Load("test-case/01.json");
var pages = PicoPDF.Binder.SectionBinder.Bind(pagesection, csv);
PicoPDF.Model.ModelMapping.Mapping(doc, pages);
doc.Save("@a.pdf", new() { ContentsStreamDeflate = false });

var png = ImageLoader.FromFile("test-case/300x150.png")!.Cast<IImageCanvas>();
new PicoPDF.Image.Bmp.BmpFile()
{
    Width = png.Width,
    Height = png.Height,
    Canvas = png.Canvas,
}.Write(File.Create("@b.bmp", 1024, FileOptions.WriteThrough));