using BenchmarkDotNet.Attributes;
using Mina.Extension;
using Pdf.Font;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace PicoPDF.Benchmark;

public class SinglePageBench
{
    public const string PageJson = """
{
	"Size": "A4",
	"Orientation": "Vertical",
	"DefaultFont": "Meiryo Bold",
	"Padding": [15, 10, 15],
	
	"Header": "PageHeader",
	"Detail": "Detail",
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 50, "ViewMode": "PageFirst", "Elements": [
			{"Type": "TextElement", "Text": "PageHeader", "Size": 30, "X": 10, "Y": 0},
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 20, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Bar",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Baz",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 30, "Elements": [
			{"Type": "TextElement", "Text": "PageFooter", "Size": 20, "X": 10,  "Y": 0, "Font": "HGMinchoB"},
			{"Type": "SummaryElement",           "Size": 10, "X": 300, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "End",   "Width": 50},
			{"Type": "TextElement", "Text": "/", "Size": 10, "X": 352, "Y": 0},
			{"Type": "SummaryElement",           "Size": 10, "X": 360, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "Start", "Width": 50, "SummaryMethod": "All"},
		]},
	],
}
""";

    public class DataLine
    {
        public required int Foo { get; init; }
        public required long Bar { get; init; }
        public required string Baz { get; init; }
    }

    public static FontRegister FontRegister { get; } = new FontRegister().Return(x => x.RegisterDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts")));

    public static readonly int[] Line1_Data = [1];
    public static readonly int[] Line1K_Data = [.. Lists.Sequence(1).Take(1_000)];
    public static readonly Dictionary<string, Func<int, object>> LineMapper = new() { ["Foo"] = (x) => x, ["Bar"] = (x) => (long)(x * 1000), ["Baz"] = (x) => x.ToString() };

    [Benchmark]
    public void Line1()
    {
        var document = PdfFactory.Create(PageJson, Line1_Data, LineMapper, new() { CreateFontRegister = () => FontRegister });
        using var mem = new MemoryStream();
        document.Save(mem);
    }

    [Benchmark]
    public void Line1K()
    {
        var document = PdfFactory.Create(PageJson, Line1K_Data, LineMapper, new() { CreateFontRegister = () => FontRegister });
        using var mem = new MemoryStream();
        document.Save(mem);
    }

    public static readonly DataLine[] Mapper1_Data = [new DataLine() { Foo = 1, Bar = 1000, Baz = "1" }];
    public static readonly DataLine[] Mapper1K_Data = [.. Lists.Sequence(1).Take(1_000).Select(x => new DataLine() { Foo = x, Bar = x * 1000, Baz = x.ToString() })];

    [Benchmark]
    public void Mapper1()
    {
        var document = PdfFactory.Create(PageJson, Mapper1_Data, null, new() { CreateFontRegister = () => FontRegister });
        using var mem = new MemoryStream();
        document.Save(mem);
    }

    [Benchmark]
    public void Mapper1K()
    {
        var document = PdfFactory.Create(PageJson, Mapper1K_Data, null, new() { CreateFontRegister = () => FontRegister });
        using var mem = new MemoryStream();
        document.Save(mem);
    }

    public static readonly DataTable DataTable1_Data = new DataTable()
        .Return(x =>
        {
            _ = x.Columns.Add("Foo");
            _ = x.Columns.Add("Bar");
            _ = x.Columns.Add("Baz");
            x.Rows.Add(x.NewRow().Return(x => { x["Foo"] = 1; x["Bar"] = (long)1000; x["Baz"] = "1"; }));
        });
    public static readonly DataTable DataTable1K_Data = new DataTable()
        .Return(x =>
        {
            _ = x.Columns.Add("Foo");
            _ = x.Columns.Add("Bar");
            _ = x.Columns.Add("Baz");
            Lists.Sequence(1).Take(1_000).Each(i => x.Rows.Add(x.NewRow().Return(x => { x["Foo"] = i; x["Bar"] = (long)(i * 1000); x["Baz"] = i.ToString(); })));
        });

    [Benchmark]
    public void DataTable1()
    {
        var document = PdfFactory.Create(PageJson, DataTable1_Data, new() { CreateFontRegister = () => FontRegister });
        using var mem = new MemoryStream();
        document.Save(mem);
    }

    [Benchmark]
    public void DataTable1K()
    {
        var document = PdfFactory.Create(PageJson, DataTable1K_Data, new() { CreateFontRegister = () => FontRegister });
        using var mem = new MemoryStream();
        document.Save(mem);
    }
}
