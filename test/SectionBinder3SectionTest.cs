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

public class SectionBinder3SectionTest
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
	        "Detail": {
			        "Detail": "Detail",
		        },
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
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 30, "Elements": [
			{"Type": "TextElement", "Text": "PageFooter", "Size": 20, "X": 10,  "Y": 0, "Font": "HGMinchoB"},
			{"Type": "SummaryElement",               "Size": 10, "X": 300, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "End",   "Width": 50},
			{"Type": "TextElement", "Text": "/", "Size": 10, "X": 352, "Y": 0},
			{"Type": "SummaryElement",               "Size": 10, "X": 360, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "Start", "Width": 50, "SummaryMethod": "All"},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 50, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "Footer1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 50, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 50, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "Footer2", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 50, "Y": 0},
		]},
	],
}
""");

    public static PageModel[] CreatePageModel(IEnumerable<(int, string, string)> datas)
    {
        var mapper = new Dictionary<string, Func<(int, string, string), object>> { ["Foo"] = (x) => x!.Item1!, ["Key1"] = (x) => x!.Item2!, ["Key2"] = (x) => x!.Item3! };
        return SectionBinder.Bind(PageSection, datas, mapper);
    }

    public static IEnumerable<(int, string, string)> MakeSectionData(string key1, string key2, int from, int to) => Lists.RangeTo(from, to).Select(x => (x, key1, key2));

    public static string ToSectionString(SectionModel section) => section.Section switch
    {
        ISection a when a is HeaderSection => $"{a.Name}/{(section.Elements[0] is TextModel t ? t.Text : "")}/{(section.Elements[1] is TextModel t2 ? t2.Text : "")}",
        ISection a when a is DetailSection => $"{a.Name}/{(section.Elements[0] is TextModel t ? t.Text : "")}",
        _ => section.Section.Name,
    };

    [Fact]
    public void Line1()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData("a", "a", 1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 7);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line2()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData("a", "a", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line1aa_1ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line1aa_1ba()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("b", "a", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/b/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/b/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line1aa_1bb()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("b", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/b/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/b/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line1aa_2ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 1).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 2));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[1].Models.Count, 7);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line6aa_1ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 6).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[1].Models.Count, 7);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line7aa_1ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 7).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[1].Models.Count, 12);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line7aa_2ab()
    {
        var i = 0;
        var datas = MakeSectionData("a", "a", 1, 7).ToList();
        datas.AddRange(MakeSectionData("a", "b", 1, 2));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[1].Models.Count, 12);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/a");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[2].Models.Count, 7);
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageHeader/PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Header1/a/b");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Header2/a/b");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Footer1");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Footer2");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageFooter");
    }
}
