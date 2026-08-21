using Mina.Command;
using Mina.Extension;
using Pdf;
using Pdf.Documents;
using Pdf.Documents.Security;
using Pdf.Extension;
using Pdf.Operation;
using PicoPDF.Model.Elements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

    [CommandOption("output-directory")]
    public string OutputDirectory { get; init; } = "test-case";

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
            BindSection = (section, page_section) =>
            {
                if (section.IsHeightAdjusting)
                {
                    var dummy_document = PdfFactory.Create(new() { CreateFontRegister = () => fontreg });
                    var multilines = section.Elements
                        .OfType<TextModel>()
                        .Where(x => x.Style.HasFlag(TextStyles.MultiLine) && !x.Style.HasFlag(TextStyles.Clipping));
                    var maxheight = multilines
                        .Select(x => x.Y + DrawString.Create(x.Text, x.X, x.Y, x.Size, [.. x.Font.Select(f => dummy_document.Resources.GetFont(f.Path, f.Option))], dummy_document, x.Width, x.Height, x.Style, x.Alignment, x.Color?.ToDeviceRGB()).Cast<DrawOperations>().Height.ToPoint())
                        .Max();
                    if (maxheight > section.Height) return section with { Height = (int)maxheight };
                }
                return section;
            },
            BindElement = (section, element, data, model) => model is TextModel text && element.Name.StartsWith("CreationTime", StringComparison.Ordinal)
                ? text with { Text = new DateTime(2000, 1, 2, 3, 45, 6, 789).ToString(text.Text, CultureInfo.InvariantCulture) }
                : model,
        };

        var datacache = new Dictionary<string, DataTable>();
        var tasks = new List<Task>();
        foreach (var json in args.Length > 0 ? args : Directory.GetFiles(WorkDirectory, "*.json"))
        {
            var fname = Path.GetFileNameWithoutExtension(json);
            var dataname = $"{WorkDirectory}/{(fname.Contains('-', StringComparison.Ordinal) ? fname[0..fname.LastIndexOf('-')] : fname)}.csv";
            var pdfname = $"{OutputDirectory}/{fname}.pdf";

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
                var opt =
                    json.Contains("AesV2", StringComparison.Ordinal) ? event_opt with { CreateStandardEncryption = () => StandardEncryption4.Create(CFM.AESV2, "xyz987", "abc123", UserAccessPermissions.Default, FixedNewBytes(16), ivgen: FixedNewBytes) } :
                    json.Contains("AesV3", StringComparison.Ordinal) ? event_opt with { CreateStandardEncryption = () => StandardEncryption6.Create(CFM.AESV3, "xyz987", "abc123", UserAccessPermissions.Default, ivgen: FixedNewBytes) } :
                    event_opt;
                var document = PdfFactory.CreateBind(json, table, opt);
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

    public static byte[] FixedNewBytes(int length) => new byte[length];
}
