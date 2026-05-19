using Binder;
using Mina.Extension;
using PicoPDF.Loader;
using PicoPDF.Model;
using PicoPDF.Model.Elements;
using PicoPDF.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

[Collection("SectionBinder")]
public class SectionBinderEventTest
{
    public static string Line5 { get; } = """
{
	"Size": [100, 50],
	"Orientation": "Vertical",
	"DefaultFont": "Meiryo-Bold",
	"Padding": [0, 10, 0],
	
	"Header": "PageHeader",
	"Detail": {
        "BreakKey": "Key1",
	    "Header": "Header1",
	    "Detail": "Detail",
	    "Footer": "Footer1",
		},
	"Footer": "PageFooter",
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
			{"Type": "TextElement", "Text": "PageHeader", "Size": 30, "X": 10, "Y": 0},
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "PageFooter", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",   "Size": 10, "X": 10,  "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "Footer1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",   "Size": 10, "X": 10,  "Y": 0},
		]},
	],
}
""";

    public static PageModel[] CreatePageModel<T>(string json, IEnumerable<(T, T)> datas, PdfEventOption? option)
    {
        var mapper = new Dictionary<string, Func<(T, T), object>> { ["Foo"] = (x) => x!.Item1!, ["Key1"] = (x) => x!.Item2! };
        return SectionBinder.Bind<(T, T), PageModel, SectionModel>(JsonLoader.CreatePageFromJson(json, option ?? new()), datas, mapper);
    }

    public static IEnumerable<(int, int)> MakeSectionData(int key1, int from, int to) => Lists.RangeTo(from, to).Select(x => (x, key1));

    public static string ToSectionString(SectionModel section) => $"{section.Section.Name}/{section.Elements.OfType<ITextModel>().Select(x => x.Text).Join("/")}";

    public static string ToSectionString2(SectionModel section) => $"{section.Section.Name},Height={section.Height}/{section.Elements.OfType<ITextModel>().Select(x => x.Text).Join("/")}";

    [Fact]
    public void NoEvent0()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 0), null);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 4);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/0");
    }

    [Fact]
    public void NoEvent1()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 1), null);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");
    }

    [Fact]
    public void NoEvent2()
    {
        // Only 5 lines per page.
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 2), null);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");

        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/100");
    }

    [Fact]
    public void NoEvent3()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 3), null);
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");

        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/100");

        i = 0;
        Assert.Equal(models[2].Models.Length, 5);
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageFooter/100");
    }

    public static readonly PdfEventOption NextNoPageHeader = new()
    {
        BindSection = (section) =>
        {
            if (section is SectionModel section_model && section_model.Section.Name == "PageHeader")
            {
                if (section_model.PageCount != 1)
                {
                    return new SectionModel
                    {
                        Section = section_model.Section,
                        Depth = section_model.Depth,
                        Top = section_model.Top,
                        Left = section_model.Left,
                        Height = section_model.Height,
                        IsFooter = section_model.IsFooter,
                        Elements = section_model.Elements,
                        PageCount = section_model.PageCount,
                        IsEmpty = section_model.IsEmpty,
                        IsVisible = false,
                    };
                }
            }
            return section;
        },
    };

    [Fact]
    public void NextNoPageHeader0()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 0), NextNoPageHeader);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 4);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/0");
    }

    [Fact]
    public void NextNoPageHeader1()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 1), NextNoPageHeader);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");
    }

    [Fact]
    public void NextNoPageHeader2()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 2), NextNoPageHeader);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");

        i = 0;
        Assert.Equal(models[1].Models.Length, 4);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/100");
    }

    [Fact]
    public void NextNoPageHeader3()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 3), NextNoPageHeader);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");

        // After hiding the PageHeader, determine the number of Detail sections.
        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/100");
    }

    [Fact]
    public void NextNoPageHeader4()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 4), NextNoPageHeader);
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");

        // After hiding the PageHeader, determine the number of Detail sections.
        i = 0;
        Assert.Equal(models[1].Models.Length, 5);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/100");

        i = 0;
        Assert.Equal(models[2].Models.Length, 4);
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Detail/4");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageFooter/100");
    }

    public static readonly PdfEventOption NextNoFooter = new()
    {
        BindSection = (section) =>
        {
            if (section is SectionModel section_model && section_model.Section.Name == "Footer1")
            {
                if (section_model.PageCount != 1)
                {
                    return new SectionModel
                    {
                        Section = section_model.Section,
                        Depth = section_model.Depth,
                        Top = section_model.Top,
                        Left = section_model.Left,
                        Height = section_model.Height,
                        IsFooter = section_model.IsFooter,
                        Elements = section_model.Elements,
                        PageCount = section_model.PageCount,
                        IsEmpty = section_model.IsEmpty,
                        IsVisible = false,
                    };
                }
            }
            return section;
        },
    };

    [Fact]
    public void NextNoFooter0()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 0), NextNoFooter);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 4);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/0");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/0");
    }

    [Fact]
    public void NextNoFooter1()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 1), NextNoFooter);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");
    }

    [Fact]
    public void NextNoFooter2()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 2), NextNoFooter);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");

        i = 0;
        Assert.Equal(models[1].Models.Length, 4);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/100");
    }

    [Fact]
    public void NextNoFooter3()
    {
        var i = 0;
        var models = CreatePageModel(Line5, MakeSectionData(100, 1, 3), NextNoFooter);
        Assert.Equal(models.Length, 3);
        Assert.Equal(models[0].Models.Length, 5);
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Detail/1");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "Footer1/100");
        Assert.Equal(ToSectionString(models[0].Models[i++]), "PageFooter/100");

        /* Determine the number of lines for the Detail section after ensuring there is space for the Footer.
           The Footer1 will be hidden, creating space for it, but you cannot insert anything there.
         */
        i = 0;
        Assert.Equal(models[1].Models.Length, 4);
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "Detail/2");
        Assert.Equal(ToSectionString(models[1].Models[i++]), "PageFooter/100");

        i = 0;
        Assert.Equal(models[2].Models.Length, 4);
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageHeader/PageHeader");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Header1/100");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "Detail/3");
        Assert.Equal(ToSectionString(models[2].Models[i++]), "PageFooter/100");
    }

    public static string Line8 { get; } = """
{
	"Size": [100, 80],
	"Orientation": "Vertical",
	"DefaultFont": "Meiryo-Bold",
	"Padding": [0, 10, 0],
	
	"Header": "PageHeader",
	"Detail": {
        "BreakKey": "Key1",
        "Header": "Header1",
        "Detail": "Detail",
        "Footer": "Footer1",
		},
	
	"Sections": [
		{"Type": "HeaderSection", "Name": "PageHeader", "Height": 10, "ViewMode": "PageFirst", "Elements": [
			{"Type": "TextElement", "Text": "PageHeader", "Size": 30, "X": 10, "Y": 0},
		]},
		{"Type": "DetailSection", "Name": "Detail", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Foo",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "HeaderSection", "Name": "Header1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1", "Name": "Key1",  "Size": 10, "X": 10, "Y": 0},
		]},
		{"Type": "FooterSection", "Name": "Footer1", "Height": 10, "Elements": [
			{"Type": "BindElement", "Bind": "Key1",   "Size": 10, "X": 10,  "Y": 0},
		]},
	],
}
""";

    public static readonly PdfEventOption HeaderExpand = new()
    {
        BindSection = (section) =>
        {
            if (section is SectionModel section_model && section_model.Section.Name == "Header1")
            {
                var key1 = section_model.Elements.OfType<ITextModel>().First(x => x.Element.Name == "Key1");

                return new SectionModel
                {
                    Section = section_model.Section,
                    Depth = section_model.Depth,
                    Top = section_model.Top,
                    Left = section_model.Left,
                    Height = int.Parse(key1.Text),
                    IsFooter = section_model.IsFooter,
                    Elements = section_model.Elements,
                    PageCount = section_model.PageCount,
                    IsEmpty = section_model.IsEmpty,
                    IsVisible = section_model.IsVisible,
                };
            }
            return section;
        },
    };

    [Fact]
    public void HeaderNoExpand1_1()
    {
        var i = 0;
        var models = CreatePageModel(Line8,
            [
                .. MakeSectionData(10, 1, 1),
                .. MakeSectionData(20, 1, 1),
            ], null);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 7);
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "PageHeader,Height=10/PageHeader");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=10/20");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/20");
    }

    [Fact]
    public void HeaderNoExpand1_2()
    {
        var i = 0;
        var models = CreatePageModel(Line8,
            [
                .. MakeSectionData(10, 1, 1),
                .. MakeSectionData(20, 1, 2),
            ], null);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 8);
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "PageHeader,Height=10/PageHeader");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=10/20");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/2");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/20");
    }

    [Fact]
    public void HeaderExpand1_1()
    {
        var i = 0;
        var models = CreatePageModel(Line8,
            [
                .. MakeSectionData(10, 1, 1),
                .. MakeSectionData(20, 1, 1),
            ], HeaderExpand);
        Assert.Equal(models.Length, 1);
        Assert.Equal(models[0].Models.Length, 7);
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "PageHeader,Height=10/PageHeader");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=20/20");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/20");
    }

    [Fact]
    public void HeaderExpand1_2()
    {
        // Header1/20 height is too large, it doesn't fit on one page, and the Detail section moves to the next page.
        var i = 0;
        var models = CreatePageModel(Line8,
            [
                .. MakeSectionData(10, 1, 1),
                .. MakeSectionData(20, 1, 2),
            ], HeaderExpand);
        Assert.Equal(models.Length, 2);
        Assert.Equal(models[0].Models.Length, 7);
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "PageHeader,Height=10/PageHeader");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/10");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Header1,Height=20/20");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Detail,Height=10/1");
        Assert.Equal(ToSectionString2(models[0].Models[i++]), "Footer1,Height=10/20");

        i = 0;
        Assert.Equal(models[1].Models.Length, 4);
        Assert.Equal(ToSectionString2(models[1].Models[i++]), "PageHeader,Height=10/PageHeader");
        Assert.Equal(ToSectionString2(models[1].Models[i++]), "Header1,Height=20/20");
        Assert.Equal(ToSectionString2(models[1].Models[i++]), "Detail,Height=10/2");
        Assert.Equal(ToSectionString2(models[1].Models[i++]), "Footer1,Height=10/20");
    }
}
