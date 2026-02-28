using Binder;
using Mina.Extension;
using PicoPDF.Loader;
using PicoPDF.Loader.Sections;
using PicoPDF.Model;
using PicoPDF.Model.Elements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class SectionBinderCultureTest
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
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0, "Format": "N2"},
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 20, "Y": 0, "Format": "N2", "Culture": "ja-JP"},
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 30, "Y": 0, "Format": "N2", "Culture": "fr-FR"},
			{"Type": "BindElement", "Bind": "Bar",  "Size": 10, "X": 40, "Y": 0, "Format": "D"},
			{"Type": "BindElement", "Bind": "Bar",  "Size": 10, "X": 50, "Y": 0, "Format": "D",  "Culture": "ja-JP"},
			{"Type": "BindElement", "Bind": "Bar",  "Size": 10, "X": 60, "Y": 0, "Format": "D",  "Culture": "fr-FR"},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 30, "Elements": [
			{"Type": "TextElement", "Text": "PageFooter", "Size": 20, "X": 10,  "Y": 0, "Font": "HGMinchoB"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 10,  "Y": 0, "Format": "N2",  "SummaryMethod": "All"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 20,  "Y": 0, "Format": "N2",  "SummaryMethod": "All", "Culture": "ja-JP"},
			{"Type": "SummaryElement", "Bind": "Foo",     "Size": 10, "X": 30,  "Y": 0, "Format": "N2",  "SummaryMethod": "All", "Culture": "fr-FR"},
			{"Type": "SummaryElement",                    "Size": 10, "X": 300, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "End",   "Width": 50},
			{"Type": "TextElement", "Text": "/",          "Size": 10, "X": 352, "Y": 0},
			{"Type": "SummaryElement",                    "Size": 10, "X": 360, "Y": 0, "Format": "#,0", "SummaryType": "PageCount", "Alignment": "Start", "Width": 50, "SummaryMethod": "All"},
		]},
	],
}
""");

    public static PageModel[] CreatePageModel<T>(IEnumerable<T> datas, DateTime d)
    {
        var count = 0;
        var mapper = new Dictionary<string, Func<T, object>> { ["Foo"] = (x) => x!, ["Bar"] = (x) => d.AddDays(count++) };
        return SectionBinder.Bind<T, PageModel, SectionModel>(PageSection, datas, mapper);
    }

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name}/{section.Elements.OfType<ITextModel>().Select(x => x.Text).Join("|")}";

    [Fact]
    public void Line0()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(1, 0), DateTime.Parse("2000/01/02", CultureInfo.InvariantCulture));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 2);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter|0.00|0.00|0,00|1|/|1");
    }

    [Fact]
    public void Line2()
    {
        var i = 0;
        var models = CreatePageModel(Lists.RangeTo(999, 1000), DateTime.Parse("2000/01/02", CultureInfo.InvariantCulture));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 4);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/999.00|999.00|999,00|Sunday, 02 January 2000|2000年1月3日月曜日|mardi 4 janvier 2000");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1,000.00|1,000.00|1 000,00|Wednesday, 05 January 2000|2000年1月6日木曜日|vendredi 7 janvier 2000");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/PageFooter|1,999.00|1,999.00|1 999,00|1|/|1");
    }
}
