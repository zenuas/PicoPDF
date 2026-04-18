using Mina.Command;
using Mina.Extension;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PicoPDF.TestAll;

public class PdfCreate : FontRegisterCommand
{
    [CommandOption("debug")]
    public bool Debug { get; init; } = true;

    [CommandOption("unicode")]
    public bool AppendCIDToUnicode { get; init; } = true;

    [CommandOption("font-embed")]
    public FontEmbed FontEmbed { get; init; } = FontEmbed.PossibleEmbed;

    [CommandOption("contents-deflate")]
    public bool ContentsStreamDeflate { get; init; } = false;

    [CommandOption("jpeg-deflate")]
    public bool JpegStreamDeflate { get; init; } = true;

    [CommandOption("image-deflate")]
    public bool ImageStreamDeflate { get; init; } = true;

    [CommandOption("cmap-deflate")]
    public bool CMapStreamDeflate { get; init; } = false;

    [CommandOption("always-update")]
    public bool AlwaysUpdate { get; init; } = true;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister(true);

        var export_opt = new PdfExportOption
        {
            Debug = Debug,
            AppendCIDToUnicode = AppendCIDToUnicode,
            FontEmbed = FontEmbed,
            ContentsStreamDeflate = ContentsStreamDeflate,
            JpegStreamDeflate = JpegStreamDeflate,
            ImageStreamDeflate = ImageStreamDeflate,
            CMapStreamDeflate = CMapStreamDeflate,
        };

        var datacache = new Dictionary<string, DataTable>();
        var tasks = new List<Task>();
        foreach (var json in args.Length > 0 ? args : Directory.GetFiles("test-case", "*.json"))
        {
            var fname = Path.GetFileNameWithoutExtension(json);
            var dataname = $"test-case/{(fname.Contains('-') ? fname[0..fname.LastIndexOf('-')] : fname)}.csv";
            var pdfname = $"test-case/{fname}.pdf";

            var pdftime = File.GetLastWriteTime(pdfname);
            if (!AlwaysUpdate && pdftime > File.GetLastWriteTime(json) && pdftime > File.GetLastWriteTime(dataname)) continue;
            Console.WriteLine(json);

            if (!datacache.TryGetValue(dataname, out var table))
            {
                var lines = File.ReadAllLines(dataname);
                table = new DataTable();
                var header = lines[0].Split(',');
                var firstfields = (lines.Length > 1 ? lines[1] : Lists.Repeat(',').Take(header.Length - 1).ToStringByChars()).Split(',');
                header.Zip(firstfields).Each(x => table.Columns.Add(x.First, AutoConvert(x.Second).GetType()));
                lines
                    .Skip(1)
                    .Select(x => x.Split(','))
                    .Each(fields => table.Rows.Add(table.NewRow().Return(row => header.Zip(fields).Each(x => row[x.First] = AutoConvert(x.Second)))));
                datacache.Add(dataname, table);
            }

            tasks.Add(Task.Run(() =>
            {
                var doc = PdfUtility.CreateDocument(json, table, fontreg);
                _ = doc.AddInfo(
                    title: fname,
                    producer: "PicoPDF for 🍣 (susi edition)",
                    creation_date: DateTime.Now,
                    mod_date: DateTime.Now,
                    trapped: "/False"
                );
                doc.Save(pdfname, export_opt);
            }));
        }
        Task.WhenAll(tasks).Wait();
    }

    public static object AutoConvert(string s) =>
        int.TryParse(s, out var n) ? n :
        DateTime.TryParse(s, out var d) ? d :
        double.TryParse(s, out var f) ? f :
        s;
}
