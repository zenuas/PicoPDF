using Binder;
using Mina.Extension;
using PicoPDF.Loader;
using PicoPDF.Loader.Sections;
using PicoPDF.Model;
using PicoPDF.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class SectionBinder3SectionKey1Test
{
    public static PageSection PageSection { get; } = JsonLoader.LoadJsonString("""
{
	"Size": "A4",
	"Orientation": "Vertical",
	"DefaultFont": "Meiryo-Bold",
	"Padding": [15, 10, 15],
	
	"Header": "PageHeader",
	"Detail": {
        "BreakKey": "Key1",
	    "Header": "Header1",
	    "Detail": {
            "BreakKey": "Key2",
	        "Header": "Header2",
	        "Detail": "Detail",
	        "Footer": "Footer2",
		    },
	    "Footer": "Footer1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 50, "ViewMode": "PageFirst", "Elements": [
			{"Type": "TextElement", "Text": "PageHeader", "Size": 30, "X": 10, "Y": 0},
			{"Type": "TextElement", "Text": "PageHeader", "Size": 10, "X": 50, "Y": 0},
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 70, "Elements": [
			{"Type": "BindElement",    "Bind": "Foo", "Size": 10, "X": 10, "Y": 0},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 100, "Y": 0, "Format": "PI-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement",                "Size": 10, "X": 110, "Y": 0, "Format": "PI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement",                "Size": 10, "X": 120, "Y": 0, "Format": "Px-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement",                "Size": 10, "X": 130, "Y": 0, "Format": "GI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "GroupIncremental", "BreakKey": "Key1"},
			{"Type": "SummaryElement",                "Size": 10, "X": 140, "Y": 0, "Format": "AI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "AllIncremental"},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 30, "Elements": [
			{"Type": "TextElement", "Text": "PageFooter", "Size": 20, "X": 10,  "Y": 0, "Font": "HGMinchoB"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 100, "Y": 0, "Format": "P-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 110, "Y": 0, "Format": "P-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 120, "Y": 0, "Format": "P-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 130, "Y": 0, "Format": "P-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 140, "Y": 0, "Format": "P-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 200, "Y": 0, "Format": "A-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 210, "Y": 0, "Format": "A-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 220, "Y": 0, "Format": "A-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 230, "Y": 0, "Format": "A-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 240, "Y": 0, "Format": "A-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement",                    "Size": 10, "X": 300, "Y": 0, "Format": "#,0",       "SummaryType": "PageCount", "Alignment": "End",   "Width": 50},
			{"Type": "TextElement", "Text": "/",          "Size": 10, "X": 352, "Y": 0},
			{"Type": "SummaryElement",                    "Size": 10, "X": 360, "Y": 0, "Format": "#,0",       "SummaryType": "PageCount", "Alignment": "Start", "Width": 50, "SummaryMethod": "All"},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 50, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Footer1", "Height": 70, "Elements": [
			{"Type": "BindElement",    "Bind": "Key1", "Size": 10, "X": 10,  "Y": 0},
			{"Type": "BindElement",    "Bind": "Key2", "Size": 10, "X": 50,  "Y": 0},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 100, "Y": 0, "Format": "P-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 110, "Y": 0, "Format": "P-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 120, "Y": 0, "Format": "P-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 130, "Y": 0, "Format": "P-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 140, "Y": 0, "Format": "P-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 200, "Y": 0, "Format": "G-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 210, "Y": 0, "Format": "G-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 220, "Y": 0, "Format": "G-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 230, "Y": 0, "Format": "G-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 240, "Y": 0, "Format": "G-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 50, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Footer2", "Height": 70, "Elements": [
			{"Type": "BindElement",    "Bind": "Key1", "Size": 10, "X": 10,  "Y": 0},
			{"Type": "BindElement",    "Bind": "Key2", "Size": 10, "X": 50,  "Y": 0},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 100, "Y": 0, "Format": "P-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 110, "Y": 0, "Format": "P-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 120, "Y": 0, "Format": "P-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 130, "Y": 0, "Format": "P-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 140, "Y": 0, "Format": "P-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 200, "Y": 0, "Format": "G-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 210, "Y": 0, "Format": "G-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 220, "Y": 0, "Format": "G-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 230, "Y": 0, "Format": "G-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 240, "Y": 0, "Format": "G-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "Group", "BreakKey": "Key1"},
		]},
	],
}
""");

    public static PageModel[] CreatePageModel(IEnumerable<(int, string, string)> datas)
    {
        var mapper = new Dictionary<string, Func<(int, string, string), object>> { ["Foo"] = (x) => x!.Item1!, ["Key1"] = (x) => x!.Item2!, ["Key2"] = (x) => x!.Item3! };
        return SectionBinder.Bind<(int, string, string), PageModel, SectionModel>(PageSection, datas, mapper);
    }

    public static IEnumerable<(int, string, string)> MakeSectionData(string key1, string key2, int from, int to) => Lists.RangeTo(from, to).Select(x => (x, key1, key2));

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name}/{section.Elements.OfType<ITextModel>().Select(x => x.Text).Join("/")}";

    [Fact]
    public void Line0()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData("a", "a", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 6);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1//");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2//");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2///P-Sum=0/P-Cnt=0/NaN///G-Sum=0/G-Cnt=0/NaN//");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1///P-Sum=0/P-Cnt=0/NaN///G-Sum=0/G-Cnt=0/NaN//");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=0/P-Cnt=0/NaN///A-Sum=0/A-Cnt=0/NaN///1///1");
    }

    [Fact]
    public void Line1()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData("a", "a", 1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 7);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=1/A-Cnt=1/A-Avg=1/A-Max=1/A-Min=1/1///1");
    }

    [Fact]
    public void Line2()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData("a", "a", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=3/P-Cnt=2/P-Avg=1/P-Max=2/P-Min=1/G-Sum=3/G-Cnt=2/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=3/P-Cnt=2/P-Avg=1/P-Max=2/P-Min=1/G-Sum=3/G-Cnt=2/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=3/P-Cnt=2/P-Avg=1/P-Max=2/P-Min=1/A-Sum=3/A-Cnt=2/A-Avg=1/A-Max=2/A-Min=1/1///1");
    }

    [Fact]
    public void Line1aa_1ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=2/G-Cnt=2/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=2/G-Cnt=2/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=2/G-Cnt=2/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=2/G-Cnt=2/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=2/A-Cnt=2/A-Avg=1/A-Max=1/A-Min=1/1///1");
    }

    [Fact]
    public void Line1aa_1ba()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("b", "a", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/b/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/b/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=1/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/b/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/b/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=2/A-Cnt=2/A-Avg=1/A-Max=1/A-Min=1/1///1");
    }

    [Fact]
    public void Line1aa_1bb()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("b", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/b/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/b/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=1/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/b/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/b/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=2/A-Cnt=2/A-Avg=1/A-Max=1/A-Min=1/1///1");
    }

    [Fact]
    public void Line1aa_2ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 2));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=4/G-Cnt=3/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=4/G-Cnt=3/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=4/G-Cnt=3/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=4/G-Cnt=3/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=4/A-Cnt=3/A-Avg=1/A-Max=2/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 7);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2/PI-Sum=2/PI-Cnt=1/Px-Cnt=1/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2/a/b/P-Sum=2/P-Cnt=1/P-Avg=2/P-Max=2/P-Min=2/G-Sum=4/G-Cnt=3/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/a/b/P-Sum=2/P-Cnt=1/P-Avg=2/P-Max=2/P-Min=2/G-Sum=4/G-Cnt=3/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=2/P-Cnt=1/P-Avg=2/P-Max=2/P-Min=2/A-Sum=4/A-Cnt=3/A-Avg=1/A-Max=2/A-Min=1/2///2");
    }

    [Fact]
    public void Line6aa_1ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 6).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/G-Sum=22/G-Cnt=7/G-Avg=3/G-Max=6/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/G-Sum=22/G-Cnt=7/G-Avg=3/G-Max=6/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/A-Sum=22/A-Cnt=7/A-Avg=3/A-Max=6/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 7);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=7/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=22/G-Cnt=7/G-Avg=3/G-Max=6/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=22/G-Cnt=7/G-Avg=3/G-Max=6/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=22/A-Cnt=7/A-Avg=3/A-Max=6/A-Min=1/2///2");
    }

    [Fact]
    public void Line7aa_1ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 7).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/G-Sum=29/G-Cnt=8/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/G-Sum=29/G-Cnt=8/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/A-Sum=29/A-Cnt=8/A-Avg=3/A-Max=7/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 12);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/7/PI-Sum=7/PI-Cnt=1/Px-Cnt=1/GI-Cnt=7/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2/a/a/P-Sum=7/P-Cnt=1/P-Avg=7/P-Max=7/P-Min=7/G-Sum=29/G-Cnt=8/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/a/a/P-Sum=7/P-Cnt=1/P-Avg=7/P-Max=7/P-Min=7/G-Sum=29/G-Cnt=8/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=8/AI-Cnt=8");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=29/G-Cnt=8/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=29/G-Cnt=8/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=29/A-Cnt=8/A-Avg=3/A-Max=7/A-Min=1/2///2");
    }

    [Fact]
    public void Line7aa_2ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 7).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 2));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2/a/a/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/a/a/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/A-Sum=31/A-Cnt=9/A-Avg=3/A-Max=7/A-Min=1/1///3");

        i = 0;
        Assert.Equal(models[1].Models.Length, 12);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/7/PI-Sum=7/PI-Cnt=1/Px-Cnt=1/GI-Cnt=7/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2/a/a/P-Sum=7/P-Cnt=1/P-Avg=7/P-Max=7/P-Min=7/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/a/a/P-Sum=7/P-Cnt=1/P-Avg=7/P-Max=7/P-Min=7/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=8/AI-Cnt=8");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/a/b/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=31/A-Cnt=9/A-Avg=3/A-Max=7/A-Min=1/2///3");

        i = 0;
        Assert.Equal(models[2].Models.Length, 7);
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Detail/2/PI-Sum=2/PI-Cnt=1/Px-Cnt=1/GI-Cnt=9/AI-Cnt=9");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Footer2/a/b/P-Sum=2/P-Cnt=1/P-Avg=2/P-Max=2/P-Min=2/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Footer1/a/b/P-Sum=2/P-Cnt=1/P-Avg=2/P-Max=2/P-Min=2/G-Sum=31/G-Cnt=9/G-Avg=3/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageFooter/PageFooter/P-Sum=2/P-Cnt=1/P-Avg=2/P-Max=2/P-Min=2/A-Sum=31/A-Cnt=9/A-Avg=3/A-Max=7/A-Min=1/3///3");
    }
}
