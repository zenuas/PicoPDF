using Mina.Command;
using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Model;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

var datacache = new Dictionary<string, DataTable>();
var fontreg = new FontRegister();
fontreg.RegistDirectory([@"test-case", Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts")]);

var (opt, jsons) = CommandLine.Run<Option>(args);
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

    var doc = new Document() { FontRegister = fontreg };
    var pagesection = JsonLoader.Load(json);
    var pages = SectionBinder.Bind(pagesection, table);
    ModelMapping.Mapping(doc, pages);
    doc.Save(pdfname, export_opt);
}

static object autoconv(string s)
{
    return
        int.TryParse(s, out var n) ? n :
        DateTime.TryParse(s, out var d) ? d :
        double.TryParse(s, out var f) ? f :
        s;
}

public class Option
{
    [CommandOption("debug")]
    public bool Debug { get; set; } = true;

    [CommandOption("unicode")]
    public bool AppendCIDToUnicode { get; set; } = true;

    [CommandOption("embedded-font")]
    public bool EmbeddedFont { get; set; } = false;

    [CommandOption("contents-deflate")]
    public bool ContentsStreamDeflate { get; set; } = false;

    [CommandOption("jpeg-deflate")]
    public bool JpegStreamDeflate { get; set; } = true;

    [CommandOption("image-deflate")]
    public bool ImageStreamDeflate { get; set; } = true;

    [CommandOption("cmap-deflate")]
    public bool CMapStreamDeflate { get; set; } = false;

    [CommandOption("always-update")]
    public bool AlwaysUpdate { get; set; } = true;
}
