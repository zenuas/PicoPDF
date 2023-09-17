using PicoPDF.Document;
using PicoPDF.Model;
using PicoPDF.Sample;
using PicoPDF.Section;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var doc = new Document();
doc.FontRegister.RegistDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts"));

var csv = File.ReadAllLines("test-case/test.csv")
    .Skip(1)
    .Select(x => x.Split(','))
    .Select(x => new Data(x[0], x[1], x[2], int.Parse(x[3])))
    .ToArray();
var pagesection = SectionLoader.Load("test-case/01.json");
var pages = SectionBinder.Bind(pagesection, csv);
ModelMapping.Mapping(doc, pages);
doc.Save("@a.pdf");
