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

var (opt, jsons) = CommandLine.Run<Option>(args);

var fontreg = new FontRegister();
if (opt.RegistSystemFont) fontreg.RegistDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts"));
if (opt.FontFileExport != "") { FontFileExport.Export(fontreg, opt); return; }
if (opt.FontList) { FontListTest.Preview(fontreg, opt); return; }

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

var datacache = new Dictionary<string, DataTable>();
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
        var firstfields = lines[1].Split(',');
        header.Zip(firstfields).Each(x => table.Columns.Add(x.First, autoconv(x.Second).GetType()));
        lines
            .Skip(1)
            .Select(x => x.Split(','))
            .Each(fields => table.Rows.Add(table.NewRow().Return(row => header.Zip(fields).Each(x => row[x.First] = autoconv(x.Second)))));
        datacache.Add(dataname, table);
    }

    PdfUtility.Create(fontreg, json, table).Save(pdfname, export_opt);
}

static object autoconv(string s) =>
    int.TryParse(s, out var n) ? n :
    DateTime.TryParse(s, out var d) ? d :
    double.TryParse(s, out var f) ? f :
    s;
