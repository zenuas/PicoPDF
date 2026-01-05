using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Binder.Data;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class SectionBinder2SectionTest
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
			    "Detail": "Detail",
		    },
	    "Footer": "Footer1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 50, "ViewMode": "PageFirst", "Elements": [
			{"Type": "TextElement", "Text": "PageHeader", "Size": 30, "X": 10, "Y": 0},
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 30, "Elements": [
			{"Type": "TextElement", "Text": "PageFooter", "Size": 20, "X": 10,  "Y": 0, "Font": "HGMinchoB"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 100, "Y": 0, "Format": "P-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 110, "Y": 0, "Format": "P-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 120, "Y": 0, "Format": "P-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 130, "Y": 0, "Format": "P-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 140, "Y": 0, "Format": "P-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 200, "Y": 0, "Format": "A-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 210, "Y": 0, "Format": "A-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 220, "Y": 0, "Format": "A-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 230, "Y": 0, "Format": "A-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 240, "Y": 0, "Format": "A-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "All"},
			{"Type": "SummaryElement",                "Size": 10, "X": 300, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "End",   "Width": 50},
			{"Type": "TextElement", "Text": "/", "Size": 10, "X": 352, "Y": 0},
			{"Type": "SummaryElement",                "Size": 10, "X": 360, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "Start", "Width": 50, "SummaryMethod": "All"},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "Footer1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 100, "Y": 0, "Format": "P-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 110, "Y": 0, "Format": "P-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 120, "Y": 0, "Format": "P-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 130, "Y": 0, "Format": "P-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 140, "Y": 0, "Format": "P-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Page"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 200, "Y": 0, "Format": "G-Sum=#,0", "SummaryType": "Summary",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Group"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 210, "Y": 0, "Format": "G-Cnt=#,0", "SummaryType": "Count",     "Alignment": "End",   "Width": 50, "SummaryMethod": "Group"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 220, "Y": 0, "Format": "G-Avg=#,0", "SummaryType": "Average",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Group"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 230, "Y": 0, "Format": "G-Max=#,0", "SummaryType": "Maximum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Group"},
			{"Type": "SummaryElement", "Bind": "Foo", "Size": 10, "X": 240, "Y": 0, "Format": "G-Min=#,0", "SummaryType": "Minimum",   "Alignment": "End",   "Width": 50, "SummaryMethod": "Group"},
		]},
	],
}
""");

    public static PageModel[] CreatePageModel<T>(IEnumerable<(T, T)> datas)
    {
        var mapper = new Dictionary<string, Func<(T, T), object>> { ["Foo"] = (x) => x!.Item1!, ["Key1"] = (x) => x!.Item2! };
        return SectionBinder.Bind(PageSection, datas, mapper);
    }

    public static IEnumerable<(int, int)> MakeSectionData(int key1, int from, int to) => Lists.RangeTo(from, to).Select(x => (x, key1));

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name}/{section.Elements.OfType<TextModel>().Select(x => x.Text).Join("/")}";

    [Fact]
    public void Line0()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData(100, 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 4);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/0/P-Sum=0/P-Cnt=0/NaN///G-Sum=0/G-Cnt=0/NaN//");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=0/P-Cnt=0/NaN///A-Sum=0/A-Cnt=0/NaN///1///1");
    }

    [Fact]
    public void Line1()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData(100, 1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=1/A-Cnt=1/A-Avg=1/A-Max=1/A-Min=1/1///1");
    }

    [Fact]
    public void Line2()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData(100, 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 6);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=3/P-Cnt=2/P-Avg=1/P-Max=2/P-Min=1/G-Sum=3/G-Cnt=2/G-Avg=1/G-Max=2/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=3/P-Cnt=2/P-Avg=1/P-Max=2/P-Min=1/A-Sum=3/A-Cnt=2/A-Avg=1/A-Max=2/A-Min=1/1///1");
    }

    [Fact]
    public void Line1_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 1).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/200/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=2/A-Cnt=2/A-Avg=1/A-Max=1/A-Min=1/1///1");
    }

    [Fact]
    public void Line5_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 5).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=15/P-Cnt=5/P-Avg=3/P-Max=5/P-Min=1/G-Sum=15/G-Cnt=5/G-Avg=3/G-Max=5/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/200/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=16/A-Cnt=6/A-Avg=2/A-Max=5/A-Min=1/1///1");
    }

    [Fact]
    public void Line6_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 6).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/G-Sum=21/G-Cnt=6/G-Avg=3/G-Max=6/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=21/P-Cnt=6/P-Avg=3/P-Max=6/P-Min=1/A-Sum=22/A-Cnt=7/A-Avg=3/A-Max=6/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Count, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=22/A-Cnt=7/A-Avg=3/A-Max=6/A-Min=1/2///2");
    }

    [Fact]
    public void Line7_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 7).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 11);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=28/P-Cnt=7/P-Avg=4/P-Max=7/P-Min=1/G-Sum=28/G-Cnt=7/G-Avg=4/G-Max=7/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=28/P-Cnt=7/P-Avg=4/P-Max=7/P-Min=1/A-Sum=29/A-Cnt=8/A-Avg=3/A-Max=7/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Count, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=29/A-Cnt=8/A-Avg=3/A-Max=7/A-Min=1/2///2");
    }

    [Fact]
    public void Line8_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 8).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=36/P-Cnt=8/P-Avg=4/P-Max=8/P-Min=1/G-Sum=36/G-Cnt=8/G-Avg=4/G-Max=8/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=36/P-Cnt=8/P-Avg=4/P-Max=8/P-Min=1/A-Sum=37/A-Cnt=9/A-Avg=4/A-Max=8/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Count, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=37/A-Cnt=9/A-Avg=4/A-Max=8/A-Min=1/2///2");
    }

    [Fact]
    public void Line9_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 9).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=36/P-Cnt=8/P-Avg=4/P-Max=8/P-Min=1/G-Sum=45/G-Cnt=9/G-Avg=5/G-Max=9/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=36/P-Cnt=8/P-Avg=4/P-Max=8/P-Min=1/A-Sum=46/A-Cnt=10/A-Avg=4/A-Max=9/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Count, 8);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/9");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100/P-Sum=9/P-Cnt=1/P-Avg=9/P-Max=9/P-Min=9/G-Sum=45/G-Cnt=9/G-Avg=5/G-Max=9/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=46/A-Cnt=10/A-Avg=4/A-Max=9/A-Min=1/2///2");
    }

    [Fact]
    public void Line10_1()
    {
        var i = 0;
        var datas = MakeSectionData(100, 1, 10).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100/P-Sum=36/P-Cnt=8/P-Avg=4/P-Max=8/P-Min=1/G-Sum=55/G-Cnt=10/G-Avg=5/G-Max=10/G-Min=1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=36/P-Cnt=8/P-Avg=4/P-Max=8/P-Min=1/A-Sum=56/A-Cnt=11/A-Avg=5/A-Max=10/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Count, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/9");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/10");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100/P-Sum=19/P-Cnt=2/P-Avg=9/P-Max=10/P-Min=9/G-Sum=55/G-Cnt=10/G-Avg=5/G-Max=10/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/200");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/200/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/G-Sum=1/G-Cnt=1/G-Avg=1/G-Max=1/G-Min=1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=56/A-Cnt=11/A-Avg=5/A-Max=10/A-Min=1/2///2");
    }
}
