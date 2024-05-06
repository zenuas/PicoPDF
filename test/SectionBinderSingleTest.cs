using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Binder.Data;
using PicoPDF.Model;
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

    [Fact]
    public void Line1()
    {
        var models = CreatePageModel(Lists.RangeTo(1, 1));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 3);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Detail");
        Assert.Equal(models[0].Models[2].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line10()
    {
        var models = CreatePageModel(Lists.RangeTo(1, 10));
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Detail");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Detail");
        Assert.Equal(models[0].Models[9].Section.Name, "Detail");
        Assert.Equal(models[0].Models[10].Section.Name, "Detail");
        Assert.Equal(models[0].Models[11].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line11()
    {
        var models = CreatePageModel(Lists.RangeTo(1, 11));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Detail");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Detail");
        Assert.Equal(models[0].Models[9].Section.Name, "Detail");
        Assert.Equal(models[0].Models[10].Section.Name, "Detail");
        Assert.Equal(models[0].Models[11].Section.Name, "PageFooter");

        Assert.Equal(models[1].Models.Count, 3);
        Assert.Equal(models[1].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[1].Models[1].Section.Name, "Detail");
        Assert.Equal(models[1].Models[2].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line20()
    {
        var models = CreatePageModel(Lists.RangeTo(1, 20));
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Detail");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Detail");
        Assert.Equal(models[0].Models[9].Section.Name, "Detail");
        Assert.Equal(models[0].Models[10].Section.Name, "Detail");
        Assert.Equal(models[0].Models[11].Section.Name, "PageFooter");

        Assert.Equal(models[1].Models.Count, 12);
        Assert.Equal(models[1].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[1].Models[1].Section.Name, "Detail");
        Assert.Equal(models[1].Models[2].Section.Name, "Detail");
        Assert.Equal(models[1].Models[3].Section.Name, "Detail");
        Assert.Equal(models[1].Models[4].Section.Name, "Detail");
        Assert.Equal(models[1].Models[5].Section.Name, "Detail");
        Assert.Equal(models[1].Models[6].Section.Name, "Detail");
        Assert.Equal(models[1].Models[7].Section.Name, "Detail");
        Assert.Equal(models[1].Models[8].Section.Name, "Detail");
        Assert.Equal(models[1].Models[9].Section.Name, "Detail");
        Assert.Equal(models[1].Models[10].Section.Name, "Detail");
        Assert.Equal(models[1].Models[11].Section.Name, "PageFooter");
    }

    [Fact]
    public void Line21()
    {
        var models = CreatePageModel(Lists.RangeTo(1, 21));
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Count, 12);
        Assert.Equal(models[0].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[0].Models[1].Section.Name, "Detail");
        Assert.Equal(models[0].Models[2].Section.Name, "Detail");
        Assert.Equal(models[0].Models[3].Section.Name, "Detail");
        Assert.Equal(models[0].Models[4].Section.Name, "Detail");
        Assert.Equal(models[0].Models[5].Section.Name, "Detail");
        Assert.Equal(models[0].Models[6].Section.Name, "Detail");
        Assert.Equal(models[0].Models[7].Section.Name, "Detail");
        Assert.Equal(models[0].Models[8].Section.Name, "Detail");
        Assert.Equal(models[0].Models[9].Section.Name, "Detail");
        Assert.Equal(models[0].Models[10].Section.Name, "Detail");
        Assert.Equal(models[0].Models[11].Section.Name, "PageFooter");

        Assert.Equal(models[1].Models.Count, 12);
        Assert.Equal(models[1].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[1].Models[1].Section.Name, "Detail");
        Assert.Equal(models[1].Models[2].Section.Name, "Detail");
        Assert.Equal(models[1].Models[3].Section.Name, "Detail");
        Assert.Equal(models[1].Models[4].Section.Name, "Detail");
        Assert.Equal(models[1].Models[5].Section.Name, "Detail");
        Assert.Equal(models[1].Models[6].Section.Name, "Detail");
        Assert.Equal(models[1].Models[7].Section.Name, "Detail");
        Assert.Equal(models[1].Models[8].Section.Name, "Detail");
        Assert.Equal(models[1].Models[9].Section.Name, "Detail");
        Assert.Equal(models[1].Models[10].Section.Name, "Detail");
        Assert.Equal(models[1].Models[11].Section.Name, "PageFooter");

        Assert.Equal(models[2].Models.Count, 3);
        Assert.Equal(models[2].Models[0].Section.Name, "PageHeader");
        Assert.Equal(models[2].Models[1].Section.Name, "Detail");
        Assert.Equal(models[2].Models[2].Section.Name, "PageFooter");
    }
}
