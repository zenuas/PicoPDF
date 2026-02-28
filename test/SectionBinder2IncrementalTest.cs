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

public class SectionBinder2IncrementalTest
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
	    "Detail": "Detail",
	    "Footer": "Footer1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 50, "ViewMode": "PageFirst", "Elements": [
			{"Type": "TextElement", "Text": "PageHeader", "Size": 30, "X": 10, "Y": 0},
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 70, "Elements": [
			{"Type": "BindElement",    "Bind": "Foo", "Size": 10, "X": 10,  "Y": 0},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 100, "Y": 0, "Format": "PI-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement",                "Size": 10, "X": 110, "Y": 0, "Format": "PI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement",                "Size": 10, "X": 120, "Y": 0, "Format": "Px-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement",                "Size": 10, "X": 130, "Y": 0, "Format": "GI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement",                "Size": 10, "X": 140, "Y": 0, "Format": "AI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "AllIncremental"},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 30, "Elements": [
			{"Type": "TextElement", "Text": "PageFooter", "Size": 20, "X": 10,  "Y": 0, "Font": "HGMinchoB"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 100, "Y": 0, "Format": "PI-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 110, "Y": 0, "Format": "PI-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 120, "Y": 0, "Format": "PI-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 130, "Y": 0, "Format": "PI-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 140, "Y": 0, "Format": "PI-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 150, "Y": 0, "Format": "Px-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 160, "Y": 0, "Format": "Px-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 170, "Y": 0, "Format": "Px-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 180, "Y": 0, "Format": "Px-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 190, "Y": 0, "Format": "Px-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 200, "Y": 0, "Format": "GI-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 210, "Y": 0, "Format": "GI-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 220, "Y": 0, "Format": "GI-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 230, "Y": 0, "Format": "GI-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 240, "Y": 0, "Format": "GI-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 250, "Y": 0, "Format": "AI-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 260, "Y": 0, "Format": "AI-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 270, "Y": 0, "Format": "AI-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 280, "Y": 0, "Format": "AI-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 290, "Y": 0, "Format": "AI-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement",                    "Size": 10, "X": 300, "Y": 0, "Format": "#,0",        "SummaryType": "PageCount", "Alignment": "End",   "Width": 50},
			{"Type": "TextElement", "Text": "/",          "Size": 10, "X": 352, "Y": 0},
			{"Type": "SummaryElement",                    "Size": 10, "X": 360, "Y": 0, "Format": "#,0",        "SummaryType": "PageCount", "Alignment": "Start", "Width": 50, "SummaryMethod": "All"},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "Footer1", "Height": 70, "Elements": [
			{"Type": "BindElement",    "Bind": "Key1", "Size": 10, "X": 10,  "Y": 0},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 100, "Y": 0, "Format": "PI-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 110, "Y": 0, "Format": "PI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 120, "Y": 0, "Format": "PI-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 130, "Y": 0, "Format": "PI-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 140, "Y": 0, "Format": "PI-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "PageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 150, "Y": 0, "Format": "Px-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 160, "Y": 0, "Format": "Px-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 170, "Y": 0, "Format": "Px-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 180, "Y": 0, "Format": "Px-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 190, "Y": 0, "Format": "Px-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "CrossSectionPageIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 200, "Y": 0, "Format": "GI-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 210, "Y": 0, "Format": "GI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 220, "Y": 0, "Format": "GI-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 230, "Y": 0, "Format": "GI-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 240, "Y": 0, "Format": "GI-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "GroupIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 250, "Y": 0, "Format": "AI-Sum=#,0", "SummaryType": "Summary", "Alignment": "End", "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 260, "Y": 0, "Format": "AI-Cnt=#,0", "SummaryType": "Count",   "Alignment": "End", "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 270, "Y": 0, "Format": "AI-Avg=#,0", "SummaryType": "Average", "Alignment": "End", "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 280, "Y": 0, "Format": "AI-Max=#,0", "SummaryType": "Maximum", "Alignment": "End", "Width": 50, "SummaryMethod": "AllIncremental"},
			{"Type": "SummaryElement", "Bind": "Foo",  "Size": 10, "X": 290, "Y": 0, "Format": "AI-Min=#,0", "SummaryType": "Minimum", "Alignment": "End", "Width": 50, "SummaryMethod": "AllIncremental"},
		]},
	],
}
""");

    public static PageModel[] CreatePageModel<T>(IEnumerable<(T, T)> datas)
    {
        var mapper = new Dictionary<string, Func<(T, T), object>> { ["Foo"] = (x) => x!.Item1!, ["Key1"] = (x) => x!.Item2! };
        return SectionBinder.Bind<(T, T), PageModel, SectionModel>(PageSection, datas, mapper);
    }

    public static IEnumerable<(int, int)> MakeSectionData(int key1, int from, int to) => Lists.RangeTo(from, to).Select(x => (x, key1));

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name}/{section.Elements.OfType<ITextModel>().Select(x => x.Text).Join("/")}";

    [Fact]
    public void Line0()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData(100, 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 4);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/0/PI-Sum=0/PI-Cnt=0/NaN///Px-Sum=0/Px-Cnt=0/NaN///GI-Sum=0/GI-Cnt=0/NaN///AI-Sum=0/AI-Cnt=0/NaN//");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=0/PI-Cnt=0/NaN///Px-Sum=0/Px-Cnt=0/NaN///GI-Sum=0/GI-Cnt=0/NaN///AI-Sum=0/AI-Cnt=0/NaN///1///1");
    }

    [Fact]
    public void Line1()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData(100, 1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=1/AI-Cnt=1/AI-Avg=1/AI-Max=1/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=1/AI-Cnt=1/AI-Avg=1/AI-Max=1/AI-Min=1/1///1");
    }

    [Fact]
    public void Line2()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData(100, 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 6);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=3/PI-Cnt=2/PI-Avg=1/PI-Max=2/PI-Min=1/Px-Sum=3/Px-Cnt=2/Px-Avg=1/Px-Max=2/Px-Min=1/GI-Sum=3/GI-Cnt=2/GI-Avg=1/GI-Max=2/GI-Min=1/AI-Sum=3/AI-Cnt=2/AI-Avg=1/AI-Max=2/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=3/PI-Cnt=2/PI-Avg=1/PI-Max=2/PI-Min=1/Px-Sum=3/Px-Cnt=2/Px-Avg=1/Px-Max=2/Px-Min=1/GI-Sum=3/GI-Cnt=2/GI-Avg=1/GI-Max=2/GI-Min=1/AI-Sum=3/AI-Cnt=2/AI-Avg=1/AI-Max=2/AI-Min=1/1///1");
    }

    [Fact]
    public void Line1_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 1).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=1/AI-Cnt=1/AI-Avg=1/AI-Max=1/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=1/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/200/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=2/Px-Cnt=2/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=2/AI-Cnt=2/AI-Avg=1/AI-Max=1/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=2/Px-Cnt=2/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=2/GI-Cnt=2/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=2/AI-Cnt=2/AI-Avg=1/AI-Max=1/AI-Min=1/1///1");
    }

    [Fact]
    public void Line5_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 5).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=15/PI-Cnt=5/PI-Avg=3/PI-Max=5/PI-Min=1/Px-Sum=15/Px-Cnt=5/Px-Avg=3/Px-Max=5/Px-Min=1/GI-Sum=15/GI-Cnt=5/GI-Avg=3/GI-Max=5/GI-Min=1/AI-Sum=15/AI-Cnt=5/AI-Avg=3/AI-Max=5/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=6/GI-Cnt=1/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/200/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=16/Px-Cnt=6/Px-Avg=2/Px-Max=5/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=16/AI-Cnt=6/AI-Avg=2/AI-Max=5/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=16/Px-Cnt=6/Px-Avg=2/Px-Max=5/Px-Min=1/GI-Sum=16/GI-Cnt=6/GI-Avg=2/GI-Max=5/GI-Min=1/AI-Sum=16/AI-Cnt=6/AI-Avg=2/AI-Max=5/AI-Min=1/1///1");
    }

    [Fact]
    public void Line6_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 6).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=21/PI-Cnt=6/PI-Avg=3/PI-Max=6/PI-Min=1/Px-Sum=21/Px-Cnt=6/Px-Avg=3/Px-Max=6/Px-Min=1/GI-Sum=21/GI-Cnt=6/GI-Avg=3/GI-Max=6/GI-Min=1/AI-Sum=21/AI-Cnt=6/AI-Avg=3/AI-Max=6/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=21/PI-Cnt=6/PI-Avg=3/PI-Max=6/PI-Min=1/Px-Sum=21/Px-Cnt=6/Px-Avg=3/Px-Max=6/Px-Min=1/GI-Sum=21/GI-Cnt=6/GI-Avg=3/GI-Max=6/GI-Min=1/AI-Sum=21/AI-Cnt=6/AI-Avg=3/AI-Max=6/AI-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=22/AI-Cnt=7/AI-Avg=3/AI-Max=6/AI-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=22/GI-Cnt=7/GI-Avg=3/GI-Max=6/GI-Min=1/AI-Sum=22/AI-Cnt=7/AI-Avg=3/AI-Max=6/AI-Min=1/2///2");
    }

    [Fact]
    public void Line7_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 7).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 11);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7/PI-Sum=28/PI-Cnt=7/Px-Cnt=7/GI-Cnt=7/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=28/PI-Cnt=7/PI-Avg=4/PI-Max=7/PI-Min=1/Px-Sum=28/Px-Cnt=7/Px-Avg=4/Px-Max=7/Px-Min=1/GI-Sum=28/GI-Cnt=7/GI-Avg=4/GI-Max=7/GI-Min=1/AI-Sum=28/AI-Cnt=7/AI-Avg=4/AI-Max=7/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=28/PI-Cnt=7/PI-Avg=4/PI-Max=7/PI-Min=1/Px-Sum=28/Px-Cnt=7/Px-Avg=4/Px-Max=7/Px-Min=1/GI-Sum=28/GI-Cnt=7/GI-Avg=4/GI-Max=7/GI-Min=1/AI-Sum=28/AI-Cnt=7/AI-Avg=4/AI-Max=7/AI-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=8");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=29/AI-Cnt=8/AI-Avg=3/AI-Max=7/AI-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=29/GI-Cnt=8/GI-Avg=3/GI-Max=7/GI-Min=1/AI-Sum=29/AI-Cnt=8/AI-Avg=3/AI-Max=7/AI-Min=1/2///2");
    }

    [Fact]
    public void Line8_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 8).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7/PI-Sum=28/PI-Cnt=7/Px-Cnt=7/GI-Cnt=7/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8/PI-Sum=36/PI-Cnt=8/Px-Cnt=8/GI-Cnt=8/AI-Cnt=8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=36/PI-Cnt=8/PI-Avg=4/PI-Max=8/PI-Min=1/Px-Sum=36/Px-Cnt=8/Px-Avg=4/Px-Max=8/Px-Min=1/GI-Sum=36/GI-Cnt=8/GI-Avg=4/GI-Max=8/GI-Min=1/AI-Sum=36/AI-Cnt=8/AI-Avg=4/AI-Max=8/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=36/PI-Cnt=8/PI-Avg=4/PI-Max=8/PI-Min=1/Px-Sum=36/Px-Cnt=8/Px-Avg=4/Px-Max=8/Px-Min=1/GI-Sum=36/GI-Cnt=8/GI-Avg=4/GI-Max=8/GI-Min=1/AI-Sum=36/AI-Cnt=8/AI-Avg=4/AI-Max=8/AI-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=9");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=37/AI-Cnt=9/AI-Avg=4/AI-Max=8/AI-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=1/Px-Cnt=1/Px-Avg=1/Px-Max=1/Px-Min=1/GI-Sum=37/GI-Cnt=9/GI-Avg=4/GI-Max=8/GI-Min=1/AI-Sum=37/AI-Cnt=9/AI-Avg=4/AI-Max=8/AI-Min=1/2///2");
    }

    [Fact]
    public void Line9_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 9).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7/PI-Sum=28/PI-Cnt=7/Px-Cnt=7/GI-Cnt=7/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8/PI-Sum=36/PI-Cnt=8/Px-Cnt=8/GI-Cnt=8/AI-Cnt=8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=36/PI-Cnt=8/PI-Avg=4/PI-Max=8/PI-Min=1/Px-Sum=36/Px-Cnt=8/Px-Avg=4/Px-Max=8/Px-Min=1/GI-Sum=36/GI-Cnt=8/GI-Avg=4/GI-Max=8/GI-Min=1/AI-Sum=36/AI-Cnt=8/AI-Avg=4/AI-Max=8/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=36/PI-Cnt=8/PI-Avg=4/PI-Max=8/PI-Min=1/Px-Sum=36/Px-Cnt=8/Px-Avg=4/Px-Max=8/Px-Min=1/GI-Sum=36/GI-Cnt=8/GI-Avg=4/GI-Max=8/GI-Min=1/AI-Sum=36/AI-Cnt=8/AI-Avg=4/AI-Max=8/AI-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 8);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/9/PI-Sum=9/PI-Cnt=1/Px-Cnt=1/GI-Cnt=9/AI-Cnt=9");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100/PI-Sum=9/PI-Cnt=1/PI-Avg=9/PI-Max=9/PI-Min=9/Px-Sum=9/Px-Cnt=1/Px-Avg=9/Px-Max=9/Px-Min=9/GI-Sum=45/GI-Cnt=9/GI-Avg=5/GI-Max=9/GI-Min=1/AI-Sum=45/AI-Cnt=9/AI-Avg=5/AI-Max=9/AI-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=2/GI-Cnt=1/AI-Cnt=10");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=10/Px-Cnt=2/Px-Avg=5/Px-Max=9/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=46/AI-Cnt=10/AI-Avg=4/AI-Max=9/AI-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=10/Px-Cnt=2/Px-Avg=5/Px-Max=9/Px-Min=1/GI-Sum=46/GI-Cnt=10/GI-Avg=4/GI-Max=9/GI-Min=1/AI-Sum=46/AI-Cnt=10/AI-Avg=4/AI-Max=9/AI-Min=1/2///2");
    }

    [Fact]
    public void Line10_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 10).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=1/GI-Cnt=1/AI-Cnt=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2/PI-Sum=3/PI-Cnt=2/Px-Cnt=2/GI-Cnt=2/AI-Cnt=2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3/PI-Sum=6/PI-Cnt=3/Px-Cnt=3/GI-Cnt=3/AI-Cnt=3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4/PI-Sum=10/PI-Cnt=4/Px-Cnt=4/GI-Cnt=4/AI-Cnt=4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5/PI-Sum=15/PI-Cnt=5/Px-Cnt=5/GI-Cnt=5/AI-Cnt=5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6/PI-Sum=21/PI-Cnt=6/Px-Cnt=6/GI-Cnt=6/AI-Cnt=6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7/PI-Sum=28/PI-Cnt=7/Px-Cnt=7/GI-Cnt=7/AI-Cnt=7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8/PI-Sum=36/PI-Cnt=8/Px-Cnt=8/GI-Cnt=8/AI-Cnt=8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/PI-Sum=36/PI-Cnt=8/PI-Avg=4/PI-Max=8/PI-Min=1/Px-Sum=36/Px-Cnt=8/Px-Avg=4/Px-Max=8/Px-Min=1/GI-Sum=36/GI-Cnt=8/GI-Avg=4/GI-Max=8/GI-Min=1/AI-Sum=36/AI-Cnt=8/AI-Avg=4/AI-Max=8/AI-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/PI-Sum=36/PI-Cnt=8/PI-Avg=4/PI-Max=8/PI-Min=1/Px-Sum=36/Px-Cnt=8/Px-Avg=4/Px-Max=8/Px-Min=1/GI-Sum=36/GI-Cnt=8/GI-Avg=4/GI-Max=8/GI-Min=1/AI-Sum=36/AI-Cnt=8/AI-Avg=4/AI-Max=8/AI-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/9/PI-Sum=9/PI-Cnt=1/Px-Cnt=1/GI-Cnt=9/AI-Cnt=9");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/10/PI-Sum=19/PI-Cnt=2/Px-Cnt=2/GI-Cnt=10/AI-Cnt=10");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100/PI-Sum=19/PI-Cnt=2/PI-Avg=9/PI-Max=10/PI-Min=9/Px-Sum=19/Px-Cnt=2/Px-Avg=9/Px-Max=10/Px-Min=9/GI-Sum=55/GI-Cnt=10/GI-Avg=5/GI-Max=10/GI-Min=1/AI-Sum=55/AI-Cnt=10/AI-Avg=5/AI-Max=10/AI-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1/PI-Sum=1/PI-Cnt=1/Px-Cnt=3/GI-Cnt=1/AI-Cnt=11");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=20/Px-Cnt=3/Px-Avg=6/Px-Max=10/Px-Min=1/GI-Sum=1/GI-Cnt=1/GI-Avg=1/GI-Max=1/GI-Min=1/AI-Sum=56/AI-Cnt=11/AI-Avg=5/AI-Max=10/AI-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/PI-Sum=1/PI-Cnt=1/PI-Avg=1/PI-Max=1/PI-Min=1/Px-Sum=20/Px-Cnt=3/Px-Avg=6/Px-Max=10/Px-Min=1/GI-Sum=56/GI-Cnt=11/GI-Avg=5/GI-Max=10/GI-Min=1/AI-Sum=56/AI-Cnt=11/AI-Avg=5/AI-Max=10/AI-Min=1/2///2");
    }
}
