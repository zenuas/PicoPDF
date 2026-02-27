using BenchmarkDotNet.Attributes;
using Binder;
using Mina.Extension;
using PicoPDF.Loader;
using PicoPDF.Loader.Section;
using PicoPDF.Model;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace PicoPDF.Benchmark;

public class SinglePageBench
{
    public static PageSection PageSection { get; } = JsonLoader.LoadJsonString("""
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
""");

    public class DataLine
    {
        public required int Foo { get; init; }
        public required long Bar { get; init; }
        public required string Baz { get; init; }
    }

    public static FontRegister FontRegister { get; } = new FontRegister().Return(x => x.RegisterDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts")));

    public static Document CreateSinglePage<T>(IEnumerable<T> datas, Dictionary<string, Func<T, object>> mapper)
    {
        var doc = new Document() { FontRegister = FontRegister };
        var pages = SectionBinder.Bind<T, SectionModel, PageModel>(PageSection, datas, mapper);
        ModelMapping.Mapping(doc, pages);
        return doc;
    }

    public static Document CreateSinglePage<T>(IEnumerable<T> datas)
    {
        var doc = new Document() { FontRegister = FontRegister };
        var pages = SectionBinder.Bind<T, SectionModel, PageModel>(PageSection, datas);
        ModelMapping.Mapping(doc, pages);
        return doc;
    }

    public static Document CreateSinglePage(DataTable table)
    {
        var doc = new Document() { FontRegister = FontRegister };
        var pages = SectionBinder.Bind<SectionModel, PageModel>(PageSection, table);
        ModelMapping.Mapping(doc, pages);
        return doc;
    }

    [Benchmark]
    public void Line1()
    {
        var mapper = new Dictionary<string, Func<int, object>> { ["Foo"] = (x) => x, ["Bar"] = (x) => (long)(x * 1000), ["Baz"] = (x) => x.ToString() };
        var doc = CreateSinglePage([1], mapper);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void Line1K()
    {
        var mapper = new Dictionary<string, Func<int, object>> { ["Foo"] = (x) => x, ["Bar"] = (x) => (long)(x * 1000), ["Baz"] = (x) => x.ToString() };
        var doc = CreateSinglePage(Lists.Sequence(1).Take(1_000), mapper);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void Mapper1()
    {
        var doc = CreateSinglePage([new DataLine() { Foo = 1, Bar = 1000, Baz = "1" }]);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void Mapper1K()
    {
        var doc = CreateSinglePage(Lists.Sequence(1).Take(1_000).Select(x => new DataLine() { Foo = x, Bar = x * 1000, Baz = x.ToString() }));
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void DataTable1()
    {
        var table = new DataTable();
        _ = table.Columns.Add("Foo");
        _ = table.Columns.Add("Bar");
        _ = table.Columns.Add("Baz");
        table.Rows.Add(table.NewRow().Return(x => { x["Foo"] = 1; x["Bar"] = (long)1000; x["Baz"] = "1"; }));

        var doc = CreateSinglePage(table);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void DataTable1K()
    {
        var table = new DataTable();
        _ = table.Columns.Add("Foo");
        _ = table.Columns.Add("Bar");
        _ = table.Columns.Add("Baz");
        Lists.Sequence(1).Take(1_000).Each(i => table.Rows.Add(table.NewRow().Return(x => { x["Foo"] = i; x["Bar"] = (long)(i * 1000); x["Baz"] = i.ToString(); })));

        var doc = CreateSinglePage(table);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }
}
