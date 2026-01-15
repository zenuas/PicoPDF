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

public class SectionBinder4CrossSectionLineTest
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
                "BreakKey": "Key3",
	            "Header": "Header3",
	            "Detail": "Detail",
	            "Footer": "Footer3",
		        },
	        "Footer": "Footer2",
		    },
	    "Footer": "Footer1",
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
		{"Type": "TotalSection", "Name": "Footer1", "Height": 10, "Elements": [
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
		{"Type": "TotalSection", "Name": "Footer2", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
		{"Type": "TotalSection", "Name": "Footer3", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
			{"Type": "BindElement", "Bind": "Key2",  "Size": 10, "X": 20, "Y": 0},
			{"Type": "BindElement", "Bind": "Key3",  "Size": 10, "X": 30, "Y": 0},
		]},
	],
}
""");

    public static PageModel[] CreatePageModel(IEnumerable<(int, string, string, string)> datas)
    {
        var mapper = new Dictionary<string, Func<(int, string, string, string), object>> { ["Foo"] = (x) => x!.Item1!, ["Key1"] = (x) => x!.Item2!, ["Key2"] = (x) => x!.Item3!, ["Key3"] = (x) => x!.Item4! };
        return SectionBinder.Bind(PageSection, datas, mapper);
    }

    public static IEnumerable<(int, string, string, string)> MakeSectionData(string key1, string key2, string key3, int from, int to) => Lists.RangeTo(from, to).Select(x => (x, key1, key2, key3));

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name},top={section.Top}/{section.Elements.OfType<TextModel>().Select(x => x.Text).Join("/")}|{section.Elements.OfType<MutableLineModel>().Select(x => $"x={x.X},y={x.Y},width={x.Width},height={x.Height}").Join("/")}";

    [Fact]
    public void Line2()
    {
        var i = 0;
        var models = CreatePageModel(MakeSectionData("a", "b", "c", 1, 2));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 10);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader,top=15/|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1,top=25/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header2,top=35/a/b/c|x=60,y=5,width=100,height=10");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header3,top=45/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=55/1|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail,top=355/2|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer3,top=655/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer2,top=665/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1,top=675/a/b/c|");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter,top=817/|");
    }
}
