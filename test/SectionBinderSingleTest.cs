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

public class SectionBinderSingleTest
{
    public static PageSection PageSection { get; } = JsonLoader.LoadJsonString("""
{
	"Size": "A4",
	"Orientation": "Vertical",
	"DefaultFont": "Meiryo-Bold",
	"Padding": [15, 10, 15],
	
	"Header": "PageHeader",
	"Detail": "Detail",
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
	],
}
""");

    public static PageModel[] CreatePageModel<T>(IEnumerable<T> datas)
    {
        var mapper = new Dictionary<string, Func<T, object>> { ["Foo"] = (x) => x! };
        return SectionBinder.Bind<T, SectionModel, PageModel>(PageSection, datas, mapper);
    }

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name}/{section.Elements.OfType<ITextModel>().Select(x => x.Text).Join("/")}";

    [Fact]
    public void Line0()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 0));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 2);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=0/P-Cnt=0/NaN///A-Sum=0/A-Cnt=0/NaN///1///1");
    }

    [Fact]
    public void Line1()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 3);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=1/P-Cnt=1/P-Avg=1/P-Max=1/P-Min=1/A-Sum=1/A-Cnt=1/A-Avg=1/A-Max=1/A-Min=1/1///1");
    }

    [Fact]
    public void Line10()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 10));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 12);
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
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=55/P-Cnt=10/P-Avg=5/P-Max=10/P-Min=1/A-Sum=55/A-Cnt=10/A-Avg=5/A-Max=10/A-Min=1/1///1");
    }

    [Fact]
    public void Line11()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 11));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
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
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=55/P-Cnt=10/P-Avg=5/P-Max=10/P-Min=1/A-Sum=66/A-Cnt=11/A-Avg=6/A-Max=11/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 3);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/11");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=11/P-Cnt=1/P-Avg=11/P-Max=11/P-Min=11/A-Sum=66/A-Cnt=11/A-Avg=6/A-Max=11/A-Min=1/2///2");
    }

    [Fact]
    public void Line20()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 20));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 12);
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
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=55/P-Cnt=10/P-Avg=5/P-Max=10/P-Min=1/A-Sum=210/A-Cnt=20/A-Avg=10/A-Max=20/A-Min=1/1///2");

        i = 0;
        Assert.Equal(models[1].Models.Length, 12);
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
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=155/P-Cnt=10/P-Avg=15/P-Max=20/P-Min=11/A-Sum=210/A-Cnt=20/A-Avg=10/A-Max=20/A-Min=1/2///2");
    }

    [Fact]
    public void Line21()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 21));
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Length, 12);
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
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter/P-Sum=55/P-Cnt=10/P-Avg=5/P-Max=10/P-Min=1/A-Sum=231/A-Cnt=21/A-Avg=11/A-Max=21/A-Min=1/1///3");

        i = 0;
        Assert.Equal(models[1].Models.Length, 12);
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
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/PageFooter/P-Sum=155/P-Cnt=10/P-Avg=15/P-Max=20/P-Min=11/A-Sum=231/A-Cnt=21/A-Avg=11/A-Max=21/A-Min=1/2///3");

        i = 0;
        Assert.Equal(models[2].Models.Length, 3);
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Detail/21");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageFooter/PageFooter/P-Sum=21/P-Cnt=1/P-Avg=21/P-Max=21/P-Min=21/A-Sum=231/A-Cnt=21/A-Avg=11/A-Max=21/A-Min=1/3///3");
    }
}
