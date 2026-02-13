using Mina.Command;
using Mina.Extension;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using PicoPDF.TestAll;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

var (opt, jsons) = CommandLine.Run<Option>(args);

if (opt.InputDeflate != "") { DeflateTest.Deflate(opt); return; }
var fontreg = (IFontRegister)(opt.FontList || opt.CMapList ? new FontRegister() : new FontRegisterLock());
if (opt.RegisterSystemFont) fontreg.RegisterDirectory([.. FontRegister.GetFontDirectories()]);
if (opt.RegisterUserFont != "") fontreg.RegisterDirectory(new PicoPDF.OpenType.LoadOption() { ForceEmbedded = true }, opt.RegisterUserFont);
if (opt.FontFileExtract != "") { FontFileExport.Export(fontreg.LoadComplete(fontreg.LoadRequiredTables(opt.FontFileExtract)), opt); return; }
if (opt.FontList) { FontListTest.Preview(fontreg.Cast<FontRegister>(), opt); return; }
if (opt.CMapList) { CMapListTest.Preview(fontreg.Cast<FontRegister>(), opt); return; }
if (opt.FontDump != "") { FontDump.Dump(fontreg.LoadRequiredTables(opt.FontDump), opt); return; }

var export_opt = new PdfExportOption
{
    Debug = opt.Debug,
    AppendCIDToUnicode = opt.AppendCIDToUnicode,
    EmbeddedFont = opt.EmbeddedFont,
    ContentsStreamDeflate = opt.ContentsStreamDeflate,
    JpegStreamDeflate = opt.JpegStreamDeflate,
    ImageStreamDeflate = opt.ImageStreamDeflate,
    CMapStreamDeflate = opt.CMapStreamDeflate,
};
ManualCreate.Run(fontreg, export_opt);

var datacache = new Dictionary<string, DataTable>();
var tasks = new List<Task>();
foreach (var json in jsons.Length > 0 ? jsons : Directory.GetFiles("test-case", "*.json"))
{
    var fname = Path.GetFileNameWithoutExtension(json);
    var dataname = $"test-case/{(fname.Contains('-') ? fname[0..fname.LastIndexOf('-')] : fname)}.csv";
    var pdfname = $"test-case/{fname}.pdf";

    var pdftime = File.GetLastWriteTime(pdfname);
    if (!opt.AlwaysUpdate && pdftime > File.GetLastWriteTime(json) && pdftime > File.GetLastWriteTime(dataname)) continue;
    Console.WriteLine(json);

    if (!datacache.TryGetValue(dataname, out var table))
    {
        var lines = File.ReadAllLines(dataname);
        table = new DataTable();
        var header = lines[0].Split(',');
        var firstfields = (lines.Length > 1 ? lines[1] : Lists.Repeat(',').Take(header.Length - 1).ToStringByChars()).Split(',');
        header.Zip(firstfields).Each(x => table.Columns.Add(x.First, autoconv(x.Second).GetType()));
        lines
            .Skip(1)
            .Select(x => x.Split(','))
            .Each(fields => table.Rows.Add(table.NewRow().Return(row => header.Zip(fields).Each(x => row[x.First] = autoconv(x.Second)))));
        datacache.Add(dataname, table);
    }

    tasks.Add(Task.Run(() =>
    {
        var doc = PdfUtility.CreateDocument(json, table, fontreg);
        doc.AddInfo(
            title: fname,
            producer: "PicoPDF for 🍣 (susi edition)",
            creation_date: DateTime.Now,
            mod_date: DateTime.Now,
            trapped: "/False"
        );
        doc.Save(pdfname, export_opt);

        if (opt.OutputFontFile != "")
        {
            foreach (var x in doc.PdfObjects.OfType<Type0Font>())
            {
                if (x.EmbeddedFont is { }) FontFileExport.Export(x.EmbeddedFont, new Option() { OutputFontFile = opt.OutputFontFile, FontExportChars = x.Chars.ToStringByChars() });
            }
        }
    }));
}
await Task.WhenAll(tasks);

static object autoconv(string s) =>
    int.TryParse(s, out var n) ? n :
    DateTime.TryParse(s, out var d) ? d :
    double.TryParse(s, out var f) ? f :
    s;
