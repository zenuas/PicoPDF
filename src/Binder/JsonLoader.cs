using Mina.Extension;
using PicoPDF.Binder.Data;
using PicoPDF.Binder.Element;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PicoPDF.Binder;

public static class JsonLoader
{
    public static PageSection Load(string path) => LoadJsonString(File.ReadAllText(path));

    public static PageSection LoadJsonString(string json) => LoadJson(JsonNode.Parse(json, null, new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })!);

    public static PageSection LoadJson(JsonNode json)
    {
        var sections = json["Sections"]!
            .AsArray()
            .Select(x => LoadSubSection(x!))
            .ToDictionary(x => x.Name, x => x);

        return new()
        {
            Size = Enum.Parse<PageSize>(json["Size"]!.ToString()),
            Orientation = Enum.Parse<Orientation>(json["Orientation"]!.ToString()),
            DefaultFont = json["DefaultFont"]!.ToString(),
            Header = json["Header"] is { } p1 ? sections[p1.ToString()].Cast<IHeaderSection>() : null,
            Footer = json["Footer"] is { } p2 ? sections[p2.ToString()].Cast<IFooterSection>() : null,
            SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json["Detail"]!.ToString()].Cast<ISubSection>(),
            Padding = LoadAllSides(json["Padding"]),
            DefaultCulture = json["DefaultCulture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : CultureInfo.InvariantCulture,
        };
    }

    public static Section LoadSection(JsonNode json, Dictionary<string, ISection> sections) => new()
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
        var pagebreak = json["PageBreak"]?.AsValue() is { } pb && (bool)pb;
        return json["Type"]!.ToString() switch
        {
            "HeaderSection" => new HeaderSection() { Name = name, Height = height, Elements = elements, ViewMode = viewmode },
            "DetailSection" => new DetailSection() { Name = name, Height = height, Elements = elements },
            "TotalSection" => new TotalSection() { Name = name, Height = height, Elements = elements, ViewMode = viewmode, PageBreak = pagebreak },
            "FooterSection" => new FooterSection() { Name = name, Height = height, Elements = elements, ViewMode = viewmode, PageBreak = pagebreak },
            _ => throw new(),
        };
    }

    public static IElement LoadElement(JsonNode json)
    {
        var posx = (int)json["X"]!.AsValue();
        var posy = (int)json["Y"]!.AsValue();
        switch (json["Type"]!.ToString())
        {
            case "TextElement":
                {
                    return new TextElement()
                    {
                        X = posx,
                        Y = posy,
                        Text = json["Text"]!.ToString(),
                        Font = json["Font"]?.ToString() ?? "",
                        Size = (int)json["Size"]!.AsValue(),
                        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignment>(align.ToString()) : TextAlignment.Start,
                        Style = json["Style"] is { } style ? Enum.Parse<TextStyle>(style.ToString()) : TextStyle.None,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                    };
                }

            case "BindElement":
                {
                    return new BindElement()
                    {
                        X = posx,
                        Y = posy,
                        Bind = json["Bind"]!.ToString(),
                        Format = json["Format"]?.ToString() ?? "",
                        Font = json["Font"]?.ToString() ?? "",
                        Size = (int)json["Size"]!.AsValue(),
                        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignment>(align.ToString()) : TextAlignment.Start,
                        Style = json["Style"] is { } style ? Enum.Parse<TextStyle>(style.ToString()) : TextStyle.None,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        Culture = json["Culture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : null,
                    };
                }

            case "SummaryElement":
                {
                    var sumtype = json["SummaryType"] is { } sum ? Enum.Parse<SummaryType>(sum.ToString()) : SummaryType.Summary;
                    return new SummaryElement()
                    {
                        X = posx,
                        Y = posy,
                        Bind = json["Bind"]?.ToString() ?? "",
                        Format = json["Format"]?.ToString() ?? "",
                        Font = json["Font"]?.ToString() ?? "",
                        Size = (int)json["Size"]!.AsValue(),
                        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignment>(align.ToString()) : TextAlignment.Start,
                        Style = json["Style"] is { } style ? Enum.Parse<TextStyle>(style.ToString()) : TextStyle.None,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        SummaryType = sumtype,
                        SummaryMethod = json["SummaryMethod"] is { } method ? Enum.Parse<SummaryMethod>(method.ToString()) : (sumtype == SummaryType.PageCount ? SummaryMethod.Page : SummaryMethod.Group),
                        BreakKey = json["BreakKey"]?.ToString() ?? "",
                        Culture = json["Culture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : null,
                    };
                }

            case "LineElement":
                {
                    return new LineElement()
                    {
                        X = posx,
                        Y = posy,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
                    };
                }

            case "CrossSectionLineElement":
                {
                    return new CrossSectionLineElement()
                    {
                        X = posx,
                        Y = posy,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
                    };
                }

            case "RectangleElement":
                {
                    return new RectangleElement()
                    {
                        X = posx,
                        Y = posy,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
                    };
                }

            case "CrossSectionRectangleElement":
                {
                    return new CrossSectionRectangleElement()
                    {
                        X = posx,
                        Y = posy,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
                    };
                }

            case "FillRectangleElement":
                {
                    return new FillRectangleElement()
                    {
                        X = posx,
                        Y = posy,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        LineColor = ColorTranslator.FromHtml(json["LineColor"]!.ToString()),
                        FillColor = ColorTranslator.FromHtml(json["FillColor"]!.ToString()),
                        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
                    };
                }

            case "CrossSectionFillRectangleElement":
                {
                    return new CrossSectionFillRectangleElement()
                    {
                        X = posx,
                        Y = posy,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        LineColor = ColorTranslator.FromHtml(json["LineColor"]!.ToString()),
                        FillColor = ColorTranslator.FromHtml(json["FillColor"]!.ToString()),
                        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
                    };
                }

            case "ImageElement":
                {
                    return new ImageElement()
                    {
                        X = posx,
                        Y = posy,
                        Path = json["Path"]?.ToString() ?? "",
                        Bind = json["Bind"]?.ToString() ?? "",
                        ZoomWidth = json["ZoomWidth"]?.AsValue() is { } zoomwidth ? (double)zoomwidth : 1.0,
                        ZoomHeight = json["ZoomHeight"]?.AsValue() is { } zoomheight ? (double)zoomheight : 1.0,
                    };
                }
        }
        throw new();
    }

    public static AllSides LoadAllSides(JsonNode? node)
    {
        if (node is JsonValue v) return new((int)v, (int)v, (int)v, (int)v);
        if (node is JsonArray xs)
        {
            switch (xs.Count)
            {
                case 1: return new((int)xs[0]!, (int)xs[0]!, (int)xs[0]!, (int)xs[0]!);
                case 2: return new((int)xs[0]!, (int)xs[0]!, (int)xs[1]!, (int)xs[1]!);
                case 3: return new((int)xs[0]!, (int)xs[2]!, (int)xs[1]!, (int)xs[1]!);
                case 4: return new((int)xs[0]!, (int)xs[2]!, (int)xs[3]!, (int)xs[1]!);
            }
        }
        return new(0, 0, 0, 0);
    }
}
