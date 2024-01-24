using BenchmarkDotNet.Attributes;
using PicoPDF.Binder;
using PicoPDF.Model;
using PicoPDF.Pdf;
using System;
using System.Collections.Generic;
using System.IO;

namespace PicoPDF.Benchmark;

public class SinglePageBench
{
    public static Document CreateSinglePage<T>(IEnumerable<T> datas)
    {
        var doc = new Document();
        doc.FontRegister.RegistDirectory(Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts"));
        var pagesection = JsonLoader.LoadJsonString(@"
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
        var mapper = new Dictionary<string, Func<T, object>> { ["Foo"] = (x) => x! };
        var pages = SectionBinder.Bind(pagesection, datas, mapper);
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
}
