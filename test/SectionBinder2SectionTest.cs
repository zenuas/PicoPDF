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
			{"Type": "SummaryElement",               "Size": 10, "X": 300, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "End",   "Width": 50},
			{"Type": "TextElement", "Text": "/", "Size": 10, "X": 352, "Y": 0},
			{"Type": "SummaryElement",               "Size": 10, "X": 360, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "Start", "Width": 50, "SummaryMethod": "All"},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "Footer1", "Height": 70, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
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

    [Fact]
    public void Line1()
    {
        var models = CreatePageModel(MakeSectionData(100, 1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 5);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[4].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line2()
    {
        var models = CreatePageModel(MakeSectionData(100, 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 6);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[5].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line1_1()
    {
        var datas = MakeSectionData(100, 1, 1).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 8);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[4].Section.Name, "Header1");
        Assert.True(models[0].Models[4].Elements[0] is TextModel h2 && h2.Text == "200");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[7].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line5_1()
    {
        var datas = MakeSectionData(100, 1, 5).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[8].Section.Name, "Header1");
        Assert.True(models[0].Models[8].Elements[0] is TextModel h2 && h2.Text == "200");
        Assert.Equal(models[0].Models[9].Section.Name, "Detail");
        Assert.Equal(models[0].Models[10].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[11].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line6_1()
    {
        var datas = MakeSectionData(100, 1, 6).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 10);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[9].Section.Name, "PageFooter");

        Assert.Equal(models[1].Models.Count, 5);
        Assert.Equal(models[1].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[1].Models[1].Section.Name, "Header1");
        Assert.True(models[1].Models[1].Elements[0] is TextModel h2 && h2.Text == "200");
        Assert.Equal(models[1].Models[2].Section.Name, "Detail");
        Assert.Equal(models[1].Models[3].Section.Name, "Footer1");
        Assert.Equal(models[1].Models[4].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line7_1()
    {
        var datas = MakeSectionData(100, 1, 7).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 11);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Detail");
        Assert.Equal(models[0].Models[9].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[10].Section.Name, "PageFooter");

        Assert.Equal(models[1].Models.Count, 5);
        Assert.Equal(models[1].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[1].Models[1].Section.Name, "Header1");
        Assert.True(models[1].Models[1].Elements[0] is TextModel h2 && h2.Text == "200");
        Assert.Equal(models[1].Models[2].Section.Name, "Detail");
        Assert.Equal(models[1].Models[3].Section.Name, "Footer1");
        Assert.Equal(models[1].Models[4].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line8_1()
    {
        var datas = MakeSectionData(100, 1, 8).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Detail");
        Assert.Equal(models[0].Models[9].Section.Name, "Detail");
        Assert.Equal(models[0].Models[10].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[11].Section.Name, "PageFooter");

        Assert.Equal(models[1].Models.Count, 5);
        Assert.Equal(models[1].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[1].Models[1].Section.Name, "Header1");
        Assert.True(models[1].Models[1].Elements[0] is TextModel h2 && h2.Text == "200");
        Assert.Equal(models[1].Models[2].Section.Name, "Detail");
        Assert.Equal(models[1].Models[3].Section.Name, "Footer1");
        Assert.Equal(models[1].Models[4].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line9_1()
    {
        var datas = MakeSectionData(100, 1, 9).ToList();
        datas.AddRange(MakeSectionData(200, 1, 1));
        var models = CreatePageModel(datas);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Header1");
        Assert.True(models[0].Models[1].Elements[0] is TextModel h1 && h1.Text == "100");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Detail");
        Assert.Equal(models[0].Models[9].Section.Name, "Detail");
        Assert.Equal(models[0].Models[10].Section.Name, "Footer1");
        Assert.Equal(models[0].Models[11].Section.Name, "PageFooter");

        Assert.Equal(models[1].Models.Count, 8);
        Assert.Equal(models[1].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[1].Models[1].Section.Name, "Header1");
        Assert.True(models[1].Models[1].Elements[0] is TextModel h2 && h2.Text == "100");
        Assert.Equal(models[1].Models[2].Section.Name, "Detail");
        Assert.Equal(models[1].Models[3].Section.Name, "Footer1");
        Assert.Equal(models[1].Models[4].Section.Name, "Header1");
        Assert.True(models[1].Models[4].Elements[0] is TextModel h3 && h3.Text == "200");
        Assert.Equal(models[1].Models[5].Section.Name, "Detail");
        Assert.Equal(models[1].Models[6].Section.Name, "Footer1");
        Assert.Equal(models[1].Models[7].Section.Name, "PageFooter");
    }
}
