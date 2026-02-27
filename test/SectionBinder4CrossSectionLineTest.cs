using Binder;
using Mina.Extension;
using PicoPDF.Loader;
using PicoPDF.Loader.Section;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class SectionBinder4CrossSectionLineTest
{
    public static PageModel[] CreatePageModel(PageSection page, IEnumerable<(int, string, string, string)> datas)
    {
        var mapper = new Dictionary<string, Func<(int, string, string, string), object>> { ["Foo"] = (x) => x!.Item1!, ["Key1"] = (x) => x!.Item2!, ["Key2"] = (x) => x!.Item3!, ["Key3"] = (x) => x!.Item4! };
        return SectionBinder.Bind<(int, string, string, string), SectionModel, PageModel>(page, datas, mapper);
    }

    public static IEnumerable<(int, string, string, string)> MakeSectionData(string key1, string key2, string key3, int from, int to) => Lists.RangeTo(from, to).Select(x => (x, key1, key2, key3));

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name},top={section.Top}/{section.Elements.OfType<ITextModel>().Select(x => x.Text).Join("/")}|{section.Elements.OfType<IPositionSizeModel>().Select(x => $"x1={x.X},y1={section.Top + x.Y},x2={x.X + x.Width},y2={section.Top + x.Y + x.Height}").Join("/")}";

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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void Line0()
    {
        var i = 0;
        var models = CreatePageModel(PageSection, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void Line2()
    {
        var i = 0;
        var models = CreatePageModel(PageSection, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void Line3()
    {
        var i = 0;
        var models = CreatePageModel(PageSection, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=370");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=375/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionAf { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "ViewMode": "First", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineAf0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAf, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineAf2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAf, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineAf3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAf, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 8);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=25/a/b/c|x1=50,y1=30,x2=150,y2=360");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=35/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=45/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=345/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionBf { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "ViewMode": "First", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineBf0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBf, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBf2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBf, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBf3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBf, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 8);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=35/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=45/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=345/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionCf { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "ViewMode": "First", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineCf0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionCf, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineCf2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionCf, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineCf3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionCf, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 8);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=360");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=45/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=345/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionAl { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineAl0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAl, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineAl2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAl, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineAl3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAl, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 9);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=370");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=375/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionBl { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineBl0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBl, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBl2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBl, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBl3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBl, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 9);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=665");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=370");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=375/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionCl { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");


    [Fact]
    public void LineCl0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionCl, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineCl2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionCl, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineCl3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionCl, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 9);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=660");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=370");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=375/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionBlCl { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineBlCl0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBlCl, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBlCl2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBlCl, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBlCl3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBlCl, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=655");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=370");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=375/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionAlBlCl { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Total2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "TotalSection", "Name": "Total2", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineAlBlCl0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAlBlCl, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=70");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=75///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineAlBlCl2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAlBlCl, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=670");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineAlBlCl3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionAlBlCl, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 7);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=655");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=370");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total2,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=375/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }

    public static PageSection PageSectionNoFooter { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
		        },
		    },
		},
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    [Fact]
    public void LineNoFooter()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionNoFooter, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 4);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=50");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
    }

    [Fact]
    public void LineNoFooter2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionNoFooter, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 6);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=655");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
    }

    [Fact]
    public void LineNoFooter3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionNoFooter, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 6);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=655");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=355");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
    }

    public static PageSection PageSectionBFl { get; } = JsonLoader.LoadJsonString("""
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Total3",
		        },
	        "Footer": "Footer2",
		    },
	    "Footer": "Total1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 300, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
			{"Type": "CrossSectionLineElement", "X": 50, "Y": 5, "Width": 100, "Height": 10},
		]},
		{"Type": "FooterSection", "Name": "Footer2", "Height": 10, "ViewMode": "Last", "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Total3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");


    [Fact]
    public void LineBFl0()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBFl, MakeSectionData("a", "b", "c", 1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35///|x1=50,y1=40,x2=150,y2=812");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=55///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=65///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2,top=807///|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBFl2()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBFl, MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=812");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2,top=807/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }

    [Fact]
    public void LineBFl3()
    {
        var i = 0;
        var models = CreatePageModel(PageSectionBFl, MakeSectionData("a", "b", "c", 1, 3));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 9);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=665"); // want y2 to be 675 or 817
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Total1,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");

        i = 0;
        Assert.Equal(models[1].Models.Length, 9);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header2,top=35/a/b/c|x1=50,y1=40,x2=150,y2=812");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail,top=55/3|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total3,top=355/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Total1,top=365/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer2,top=807/a/b/c|");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter,top=817/|");
    }
}
