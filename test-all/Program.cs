using Mina.Extensions;
using PicoPDF.Pdf.Font;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

var datacache = new Dictionary<string, DataTable>();
var fontreg = new FontRegister();
fontreg.RegistDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts"));

foreach (var json in Directory.GetFiles("test-case", "*.json"))
{
    var fname = Path.GetFileNameWithoutExtension(json);
    var dataname = $"test-case/{(fname.Contains('-') ? fname[0..fname.LastIndexOf('-')] : fname)}.csv";
    var pdfname = $"test-case/{fname}.pdf";

    if (!datacache.ContainsKey(dataname))
    {
        var lines = File.ReadAllLines(dataname);
        var table = new DataTable();
        var header = lines[0].Split(',');
        var firstfields = lines[1].Split(',');
        header.Zip(firstfields).Each(x => table.Columns.Add(x.First, autoconv(x.Second).GetType()));
        lines
            .Skip(1)
            .Select(x => x.Split(','))
            .Each(fields => table.Rows.Add(table.NewRow().Return(row => header.Zip(fields).Each(x => row[x.First] = autoconv(x.Second)))));
        datacache.Add(dataname, table);
    }
    var datas = datacache[dataname];

    var doc = new PicoPDF.Pdf.Document() { FontRegister = fontreg };
    var pagesection = PicoPDF.Binder.JsonLoader.Load(json);
    var pages = PicoPDF.Binder.SectionBinder.Bind(pagesection, datas);
    PicoPDF.Model.ModelMapping.Mapping(doc, pages);
    doc.Save(pdfname, new() { ContentsStreamDeflate = false, Debug = true });
}

static object autoconv(string s)
{
    return
        int.TryParse(s, out var n) ? n :
        DateTime.TryParse(s, out var d) ? d :
        double.TryParse(s, out var f) ? f :
        s;
}
