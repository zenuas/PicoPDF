using BenchmarkDotNet.Attributes;
using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Binder.Data;
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
    public static PageSection PageSection { get; } = JsonLoader.LoadJsonString(@"
{
	""Size"": ""A4"",
	""Orientation"": ""Vertical"",
	""DefaultFont"": ""Meiryo-Bold"",
	""Padding"": [15, 10, 15],
	
	""Header"": ""PageHeader"",
	""Detail"": {
			""Detail"": ""Detail"",
		},
	""Footer"": ""PageFooter"",
	
	""Sections"": [
		{""Type"": ""HeaderSection"", ""Name"": ""PageHeader"", ""Height"": 50, ""ViewMode"": ""PageFirst"", ""Elements"": [
			{""Type"": ""TextElement"", ""Text"": ""PageHeader"", ""Size"": 30, ""X"": 10, ""Y"": 0},
		]},
		{""Type"": ""DetailSection"", ""Name"": ""Detail"", ""Height"": 20, ""Elements"": [
			{""Type"": ""BindElement"", ""Bind"": ""Foo"",  ""Size"": 10, ""X"": 10, ""Y"": 0},
		]},
		{""Type"": ""FooterSection"", ""Name"": ""PageFooter"", ""Height"": 30, ""Elements"": [
			{""Type"": ""TextElement"", ""Text"": ""PageFooter"", ""Size"": 20, ""X"": 10,  ""Y"": 0, ""Font"": ""HGMinchoB""},
			{""Type"": ""SummaryElement"",               ""Size"": 10, ""X"": 300, ""Y"": 0, ""Format"": ""#,0"", ""SummaryType"": ""PageCount"", ""Alignment"": ""End"",   ""Width"": 50},
			{""Type"": ""TextElement"", ""Text"": ""/"", ""Size"": 10, ""X"": 352, ""Y"": 0},
			{""Type"": ""SummaryElement"",               ""Size"": 10, ""X"": 360, ""Y"": 0, ""Format"": ""#,0"", ""SummaryType"": ""PageCount"", ""Alignment"": ""Start"", ""Width"": 50, ""SummaryMethod"": ""All""},
		]},
	],
}
");

    public static FontRegister FontRegister { get; } = new FontRegister().Return(x => x.RegistDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts")));

    public static Document CreateSinglePage<T>(IEnumerable<T> datas)
    {
        var doc = new Document() { FontRegister = FontRegister };
        var mapper = new Dictionary<string, Func<T, object>> { ["Foo"] = (x) => x! };
        var pages = SectionBinder.Bind(PageSection, datas, mapper);
        ModelMapping.Mapping(doc, pages);
        return doc;
    }

    public static Document CreateSinglePage(DataTable table)
    {
        var doc = new Document() { FontRegister = FontRegister };
        var pages = SectionBinder.Bind(PageSection, table);
        ModelMapping.Mapping(doc, pages);
        return doc;
    }

    [Benchmark]
    public void Line1()
    {
        var doc = CreateSinglePage([1]);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void Line1K()
    {
        var doc = CreateSinglePage(Lists.Sequence(1).Take(1_000));
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void DataTable1()
    {
        var table = new DataTable();
        _ = table.Columns.Add("Foo");
        table.Rows.Add(table.NewRow().Return(x => x["Foo"] = 1));

        var doc = CreateSinglePage(table);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }

    [Benchmark]
    public void DataTable1K()
    {
        var table = new DataTable();
        _ = table.Columns.Add("Foo");
        Lists.Sequence(1).Take(1_000).Each(i => table.Rows.Add(table.NewRow().Return(x => x["Foo"] = i)));

        var doc = CreateSinglePage(table);
        using var mem = new MemoryStream();
        doc.Save(mem);
    }
}
