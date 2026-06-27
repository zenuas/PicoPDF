using Mina.Command;
using Mina.Extension;
using Pdf;
using Pdf.Documents;
using Pdf.Documents.BreakRule;
using Pdf.Extension;
using Pdf.Operation;
using PicoPDF.Loader.Elements;
using PicoPDF.Model;
using PicoPDF.Model.Elements;
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

    [CommandOption("output-cross-reference-table")]
    public bool OutputCrossReferenceTable { get; init; } = true;

    [CommandOption("point-format")]
    public string PointFormat { get; init; } = "F%";

    [CommandOption("work-directory")]
    public string WorkDirectory { get; init; } = "test-case";

    public override void Run(string[] args)
    {
        var export_opt = new PdfExportOption
        {
            Debug = Debug,
            AppendCIDToUnicode = AppendCIDToUnicode,
            ContentsStreamDeflate = ContentsStreamDeflate,
            JpegStreamDeflate = JpegStreamDeflate,
            ImageStreamDeflate = ImageStreamDeflate,
            CMapStreamDeflate = CMapStreamDeflate,
            OutputCrossReferenceTable = OutputCrossReferenceTable,
            PointFormat = PointFormat,
        };

        var fontreg = CreateFontRegister(true);
        var event_opt = new PdfEventOption
        {
            CreateFontRegister = () => fontreg,
            BindSection = section =>
            {
                if (section is SectionModel section_model && section_model.Section.Name.StartsWith("HeightAdjusting"))
                {
                    var dummy_document = new Document() { FontRegister = fontreg };
                    var multilines = section_model.Elements
                        .OfType<TextModel>()
                        .Where(x => x.Style.HasFlag(TextStyles.MultiLine) && !x.Style.HasFlag(TextStyles.Clipping));
                    var maxheight = multilines
                        .Select(x => x.Y + DrawString.Create(dummy_document, x.Text, x.X, x.Y, x.Size, [.. x.Font.Select(f => dummy_document.GetFont(f.Path, f.Embed))], x.Width, x.Height, x.Style, x.Alignment, x.Color?.ToDeviceRGB()).Cast<DrawOperations>().Height.ToPoint())
                        .Max();
                    if (maxheight > section_model.Height) return section_model with { Height = (int)maxheight };
                }
                return section;
            },
            BindElement = (section, element, data, model) => model is TextModel text && element.Name.StartsWith("CreationTime")
                ? text with { Text = DateTime.Now.ToString(text.Text) }
                : model,
            Mapping = (page, model, top, left) =>
            {
                if (model is ITextModel x &&
                    model.Element is ITextElement e &&
                    e.Style.HasFlag(TextStyles.LineBreak) &&
                    e.Name.StartsWith("Jp"))
                {
                    double posx = model.X + left;
                    double posy = model.Y + top;
                    return DrawString.Create(page.Document, model.Cast<ITextModel>().Text, posx, posy, x.Size, [.. x.Font.Select(x => page.Document.GetFont(x.Path, x.Embed))], x.Width, x.Height, x.Style, x.Alignment, x.Color?.ToDeviceRGB(), new JapaneseLineBreakRule());
                }
                else
                {
                    return ModelMapping.Mapping(page, model, top, left);
                }
            },
        };

        var datacache = new Dictionary<string, DataTable>();
        var tasks = new List<Task>();
        foreach (var json in args.Length > 0 ? args : Directory.GetFiles(WorkDirectory, "*.json"))
        {
            var fname = Path.GetFileNameWithoutExtension(json);
            var dataname = $"{WorkDirectory}/{(fname.Contains('-') ? fname[0..fname.LastIndexOf('-')] : fname)}.csv";
            var pdfname = $"{WorkDirectory}/{fname}.pdf";

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
                var document = PdfFactory.Create(json, table, event_opt);
                document.Save(pdfname, export_opt);
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
