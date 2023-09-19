using Extensions;
using PicoPDF.Document;
using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace PicoPDF.Section;

public static class SectionLoader
{
    public static PageSection Load(string path)
    {
        var json = JsonNode.Parse(File.ReadAllText(path))!;

        var sections = json["Sections"]!
            .AsArray()
            .Select(x => LoadSubSection(x!))
            .ToDictionary(x => x.Name, x => x);

        return new PageSection()
        {
            Size = Enum.Parse<PageSize>(json["Size"]!.ToString()),
            Orientation = Enum.Parse<Orientation>(json["Orientation"]!.ToString()),
            DefaultFont = json["DefaultFont"]!.ToString(),
            Header = json["Header"] is { } p1 ? sections[p1.ToString()].Cast<IHeaderSection>() : null,
            Footer = json["Footer"] is { } p2 ? sections[p2.ToString()].Cast<IFooterSection>() : null,
            SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json["Detail"]!.ToString()].Cast<ISubSection>(),
        };
    }

    public static Section LoadSection(JsonNode json, Dictionary<string, ISection> sections) => new Section()
    {
        BreakKey = json["BreakKey"]?.ToString() ?? "",
        Header = json["Header"] is { } p1 ? sections[p1.ToString()].Cast<IHeaderSection>() : null,
        Footer = json["Footer"] is { } p2 ? sections[p2.ToString()].Cast<IFooterSection>() : null,
        SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json["Detail"]!.ToString()].Cast<ISubSection>(),
    };

    public static ISection LoadSubSection(JsonNode json)
    {
        var elements = json["Elements"]!.AsArray().Select(x => LoadElement(x!)).ToList();
        var name = json["Name"]!.ToString();
        var height = (int)json["Height"]!.AsValue();
        var viewmode = json["ViewMode"] is { } v ? Enum.Parse<ViewModes>(v.ToString()) : ViewModes.Every;
        var pagebreak = json["PageBreak"]?.AsValue() is { } pb ? (bool)pb : false;
        var detailinclude = json["DetailInclude"]?.AsValue() is { } di ? (bool)di : true;
        switch (json["Type"]!.ToString())
        {
            case "HeaderSection":
                return new HeaderSection() { Name = name, Height = height, ViewMode = viewmode, Elements = elements, DetailInclude = detailinclude };

            case "DetailSection":
                return new DetailSection() { Name = name, Height = height, Elements = elements };

            case "TotalSection":
                return new TotalSection() { Name = name, Height = height, ViewMode = viewmode, Elements = elements, PageBreak = pagebreak, DetailInclude = detailinclude };

            case "FooterSection":
                return new FooterSection() { Name = name, Height = height, ViewMode = viewmode, Elements = elements, PageBreak = pagebreak, DetailInclude = detailinclude };
        }
        throw new();
    }

    public static ISectionElement LoadElement(JsonNode json)
    {
        var posx = (int)json["X"]!.AsValue();
        var posy = (int)json["Y"]!.AsValue();
        switch (json["Type"]!.ToString())
        {
            case "TextElement":
                return new TextElement()
                {
                    X = posx,
                    Y = posy,
                    Text = json["Text"]!.ToString(),
                    Font = json["Font"]?.ToString() ?? "",
                    Size = (int)json["Size"]!.AsValue(),
                };

            case "BindElement":
                return new BindElement()
                {
                    X = posx,
                    Y = posy,
                    Bind = json["Bind"]!.ToString(),
                    Format = json["Format"]?.ToString() ?? "",
                    Font = json["Font"]?.ToString() ?? "",
                    Size = (int)json["Size"]!.AsValue(),
                };

            case "SummaryElement":
                return new SummaryElement()
                {
                    X = posx,
                    Y = posy,
                    Bind = json["Bind"]!.ToString(),
                    Format = json["Format"]?.ToString() ?? "",
                    Font = json["Font"]?.ToString() ?? "",
                    Size = (int)json["Size"]!.AsValue(),
                    SummaryType = json["SummaryType"] is { } v ? Enum.Parse<SummaryType>(v.ToString()) : SummaryType.Summary,
                };
        }
        throw new();
    }
}
