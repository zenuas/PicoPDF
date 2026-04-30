using Binder.Data;
using Mina.Extension;
using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PicoPDF.Loader;

public static class JsonLoader
{
    public static PageSection CreatePageFromJsonFile(string path, PdfEventOption? option = null) => CreatePageFromJson(File.ReadAllText(path), option);

    public static PageSection CreatePageFromJson(string json, PdfEventOption? option = null) => CreatePageFromJson(JsonNode.Parse(json, null, new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })!, option);

    public static PageSection CreatePageFromJson(JsonNode json, PdfEventOption? option = null)
    {
        var sections = json["Sections"]!
            .AsArray()
            .Select(x => LoadSubSection(x!))
            .ToDictionary(x => x.Name, x => x);

        var fonts = ToFontPathArray(json["DefaultFont"]);
        if (fonts.Length == 0) throw new("DefaultFont is empty");

        var opt = option ?? new PdfEventOption();
        return new()
        {
            Size = LoadPageSize(json["Size"]),
            Orientation = json["Orientation"] is { } orient ? Enum.Parse<Orientation>(orient!.ToString()) : Orientation.Vertical,
            DefaultFont = fonts,
            Header = json["Header"] is { } p1 ? sections[p1.ToString()].Cast<IHeaderSection>() : null,
            Footer = json["Footer"] is { } p2 ? sections[p2.ToString()].Cast<IFooterSection>() : null,
            SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json["Detail"]!.ToString()].Cast<ISubSection>(),
            Padding = LoadAllSides(json["Padding"]),
            DefaultCulture = json["DefaultCulture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : CultureInfo.InvariantCulture,
            BindElement = opt.BindElement,
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
        var elements = json["Elements"]!.AsArray().Select(x => LoadElement(x!)).ToArray();
        var name = json["Name"]!.ToString();
        var height = (int)json["Height"]!.AsValue();
        var viewmode = json["ViewMode"] is { } v ? Enum.Parse<ViewModes>(v.ToString()) : ViewModes.Every;
        var pagebreak = json["PageBreak"]?.AsValue() is { } pb && (bool)pb;
        return json["Type"]!.ToString() switch
        {
            "HeaderSection" => new HeaderSection() { Name = name, Height = height, Elements = elements, ViewMode = viewmode },
            "DetailSection" => new DetailSection() { Name = name, Height = height, Elements = elements, ViewMode = viewmode, Fill = json["Fill"]?.AsValue() is { } fill && (bool)fill },
            "TotalSection" => new TotalSection() { Name = name, Height = height, Elements = elements, ViewMode = viewmode, PageBreak = pagebreak },
            "FooterSection" => new FooterSection() { Name = name, Height = height, Elements = elements, ViewMode = viewmode, PageBreak = pagebreak },
            _ => throw new(),
        };
    }

    public static IElement LoadElement(JsonNode json)
    {
        var posx = (int)json["X"]!.AsValue();
        var posy = (int)json["Y"]!.AsValue();
        var name = json["Name"]?.ToString() ?? "";
        switch (json["Type"]!.ToString())
        {
            case "TextElement":
                {
                    return new TextElement()
                    {
                        X = posx,
                        Y = posy,
                        Name = name,
                        Text = json["Text"]!.ToString(),
                        Font = ToFontPathArray(json["Font"]),
                        Size = (int)json["Size"]!.AsValue(),
                        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignment>(align.ToString()) : TextAlignment.Start,
                        Style = json["Style"] is { } style ? Enum.Parse<TextStyle>(style.ToString()) : TextStyle.None,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                    };
                }

            case "BindElement":
                {
                    return new BindElement()
                    {
                        X = posx,
                        Y = posy,
                        Name = name,
                        Bind = json["Bind"]!.ToString(),
                        Format = json["Format"]?.ToString() ?? "",
                        Font = ToFontPathArray(json["Font"]),
                        Size = (int)json["Size"]!.AsValue(),
                        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignment>(align.ToString()) : TextAlignment.Start,
                        Style = json["Style"] is { } style ? Enum.Parse<TextStyle>(style.ToString()) : TextStyle.None,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        Culture = json["Culture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : null,
                    };
                }

            case "SummaryElement":
                {
                    var sumtype = json["SummaryType"] is { } sum ? Enum.Parse<SummaryType>(sum.ToString()) : SummaryType.Summary;
                    var summethod = json["SummaryMethod"] is { } method ? Enum.Parse<SummaryMethod>(method.ToString()) : (sumtype == SummaryType.PageCount ? SummaryMethod.Page : SummaryMethod.Group);
                    if (json["BreakKey"] is not null && summethod is not (SummaryMethod.Group or SummaryMethod.GroupIncremental)) throw new($"when SummaryElement is SummaryMethod={summethod}, BreakKey is invalid");
                    return new SummaryElement()
                    {
                        X = posx,
                        Y = posy,
                        Name = name,
                        Bind = json["Bind"]?.ToString() ?? "",
                        Format = json["Format"]?.ToString() ?? "",
                        Font = ToFontPathArray(json["Font"]),
                        Size = (int)json["Size"]!.AsValue(),
                        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignment>(align.ToString()) : TextAlignment.Start,
                        Style = json["Style"] is { } style ? Enum.Parse<TextStyle>(style.ToString()) : TextStyle.None,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
                        SummaryType = sumtype,
                        SummaryMethod = summethod,
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
                        Name = name,
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
                        Name = name,
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
                        Name = name,
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
                        Name = name,
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
                        Name = name,
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
                        Name = name,
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
                        Name = name,
                        Path = json["Path"]?.ToString() ?? "",
                        Bind = json["Bind"]?.ToString() ?? "",
                        ZoomWidth = json["ZoomWidth"]?.AsValue() is { } zoomwidth ? (double)zoomwidth : 1.0,
                        ZoomHeight = json["ZoomHeight"]?.AsValue() is { } zoomheight ? (double)zoomheight : 1.0,
                    };
                }
        }
        throw new();
    }

    public static FontPath[] ToFontPathArray(JsonNode? node) =>
        node is JsonValue v ? [new() { Path = (string)v! }] :
        node is JsonArray xs ? [.. xs.Select(x => ToFontPath(x!))] :
        [];

    public static FontPath ToFontPath(JsonNode node) =>
        node is JsonValue v ? new() { Path = (string)v! } :
        new()
        {
            Path = node["Path"]?.ToString() ?? "",
            Embed = node["Embed"] is { } embed ? Enum.Parse<FontEmbed>(embed.ToString()) : FontEmbed.PossibleEmbed,
        };

    public static PageSize LoadPageSize(JsonNode? node)
    {
        if (node is JsonValue v) return PageSize.Parse((string)v!);
        if (node is JsonArray xs)
        {
            switch (xs.Count)
            {
                case 2: return new((int)xs[0]!, (int)xs[1]!);
            }
        }
        return new(PageSizes.A4);
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
