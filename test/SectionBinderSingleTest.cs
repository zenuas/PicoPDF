using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Binder.Data;
using PicoPDF.Model;
using PicoPDF.Model.Element;
using System;
using System.Collections.Generic;
using Xunit;

namespace PicoPDF.Test;

public class SectionBinderSingleTest
{
    public static PageSection PageSection { get; } = JsonLoader.LoadJsonString("""
{
	"Size": "A4",
	"Orientation": "Vertical",
	"DefaultFont": "Meiryo-Bold",
	"Padding": [15, 10, 15],
	
	"Header": "PageHeader",
	"Detail": {
			"Detail": "Detail",
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
	],
}
""");

    public static PageModel[] CreatePageModel<T>(IEnumerable<T> datas)
    {
        var mapper = new Dictionary<string, Func<T, object>> { ["Foo"] = (x) => x! };
        return SectionBinder.Bind(PageSection, datas, mapper);
    }

    public static string ToSectionString(SectionModel section) => section.Section switch
    {
        ISection a when a is HeaderSection => $"{a.Name}/{(section.Elements[0] is TextModel t ? t.Text : "")}",
        ISection a when a is DetailSection => $"{a.Name}/{(section.Elements[0] is TextModel t ? t.Text : "")}",
        _ => section.Section.Name,
    };

    [Fact]
    public void Line1()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 3);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line10()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 10));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/9");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/10");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line11()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 11));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/9");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/10");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[1].Models.Count, 3);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/11");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line20()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 20));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/9");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/10");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[1].Models.Count, 12);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/11");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/12");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/13");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/14");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/15");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/16");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/17");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/18");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/19");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/20");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter");
    }

    [Fact]
    public void Line21()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 21));
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/5");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/6");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/7");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/8");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/9");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/10");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[1].Models.Count, 12);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/11");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/12");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/13");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/14");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/15");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/16");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/17");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/18");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/19");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/20");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter");

        i = 0;
        Assert.Equal(models[2].Models.Count, 3);
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Detail/21");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageFooter");
    }
}
