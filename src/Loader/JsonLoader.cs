using Binder.Data;
using Mina.Extension;
using Pdf.Documents;
using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
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
    public static JsonNode ToNode(this JsonNode self, string name) => self[name] ?? throw new NullReferenceException($"Element '{name}' was not found.");

    public static PageSection CreatePageFromJsonFile(string path, PdfEventOption option) => CreatePageFromJson(File.ReadAllText(path), option);

    public static PageSection CreatePageFromJson(string json, PdfEventOption option) => CreatePageFromJson(JsonNode.Parse(json, null, new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })!, option);

    public static PageSection CreatePageFromJson(JsonNode json, PdfEventOption option)
    {
        var fonts = ToFontPathArray(json["DefaultFont"]);
        if (fonts.Length == 0) throw new("DefaultFont is empty");

        var size = LoadPageSize(json["Size"]);
        var padding = LoadAllSides(json["Padding"]);
        var sections = json.ToNode("Sections")
            .AsArray()
            .Select(x => LoadSubSection(x!, size.Width - padding.Left - padding.Right))
            .ToDictionary(x => x.Name, x => x);

        return new()
        {
            Size = size,
            Orientation = json["Orientation"] is { } orient ? Enum.Parse<Orientations>(orient!.ToString()) : Orientations.Vertical,
            DefaultFont = fonts,
            Header = json["Header"] is { } p1 ? sections[p1.ToString()].Cast<IHeaderSection>() : null,
            Footer = json["Footer"] is { } p2 ? sections[p2.ToString()].Cast<IFooterSection>() : null,
            SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.ToNode("Detail").ToString()].Cast<ISubSection>(),
            Padding = padding,
            DefaultCulture = json["DefaultCulture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : CultureInfo.InvariantCulture,
            EventOption = option,
        };
    }

    public static Section LoadSection(JsonNode json, Dictionary<string, ISection> sections) => new()
    {
        BreakKey = json["BreakKey"]?.ToString() ?? "",
        Header = json["Header"] is { } p1 ? sections[p1.ToString()].Cast<IHeaderSection>() : null,
        Footer = json["Footer"] is { } p2 ? sections[p2.ToString()].Cast<IFooterSection>() : null,
        SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.ToNode("Detail").ToString()].Cast<ISubSection>(),
    };

    public static ISection LoadSubSection(JsonNode json, int width)
    {
        var name = json.ToNode("Name").ToString();
        var height = (int)json.ToNode("Height").AsValue();
        var elements = json.ToNode("Elements").AsArray().Select(x => LoadElement(x!)).ToArray();
        var viewmode = json["ViewMode"] is { } v ? Enum.Parse<ViewModes>(v.ToString()) : ViewModes.Every;
        var style = json["Style"]?.AsValue() is { } st ? Enum.Parse<SectionStyles>(st.ToString()) : SectionStyles.None;
        return json.ToNode("Type").ToString() switch
        {
            "HeaderSection" => new HeaderSection() { Name = name, Height = height, Width = width, Elements = elements, ViewMode = viewmode, Style = style },
            "DetailSection" => new DetailSection() { Name = name, Height = height, Width = width, Elements = elements, ViewMode = viewmode, Style = style },
            "TotalSection" => new TotalSection() { Name = name, Height = height, Width = width, Elements = elements, ViewMode = viewmode, Style = style },
            "FooterSection" => new FooterSection() { Name = name, Height = height, Width = width, Elements = elements, ViewMode = viewmode, Style = style },
            _ => throw new(),
        };
    }

    public static IElement LoadElement(JsonNode json)
    {
        var posx = (int)json.ToNode("X").AsValue();
        var posy = (int)json.ToNode("Y").AsValue();
        var name = json["Name"]?.ToString() ?? "";
        return json.ToNode("Type").ToString() switch
        {
            "TextElement" => LoadTextElement(posx, posy, name, json),
            "BindElement" => LoadBindElement(posx, posy, name, json),
            "SummaryElement" => LoadSummaryElement(posx, posy, name, json),
            "LineElement" => LoadLineElement(posx, posy, name, json),
            "CrossSectionLineElement" => LoadCrossSectionLineElement(posx, posy, name, json),
            "RectangleElement" => LoadRectangleElement(posx, posy, name, json),
            "CrossSectionRectangleElement" => LoadCrossSectionRectangleElement(posx, posy, name, json),
            "FillRectangleElement" => LoadFillRectangleElement(posx, posy, name, json),
            "CrossSectionFillRectangleElement" => LoadCrossSectionFillRectangleElement(posx, posy, name, json),
            "ImageElement" => LoadImageElement(posx, posy, name, json),
            _ => throw new(),
        };
    }

    public static TextElement LoadTextElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Text = json.ToNode("Text").ToString(),
        Font = ToFontPathArray(json["Font"]),
        Size = (int)json.ToNode("Size").AsValue(),
        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignments>(align.ToString()) : TextAlignments.Start,
        Style = json["Style"] is { } style ? Enum.Parse<TextStyles>(style.ToString()) : TextStyles.None,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
    };

    public static BindElement LoadBindElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Bind = json.ToNode("Bind").ToString(),
        Format = json["Format"]?.ToString() ?? "",
        Font = ToFontPathArray(json["Font"]),
        Size = (int)json.ToNode("Size").AsValue(),
        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignments>(align.ToString()) : TextAlignments.Start,
        Style = json["Style"] is { } style ? Enum.Parse<TextStyles>(style.ToString()) : TextStyles.None,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
        Culture = json["Culture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : null,
    };

    public static SummaryElement LoadSummaryElement(int posx, int posy, string name, JsonNode json)
    {
        var sumtype = json["SummaryType"] is { } sum ? Enum.Parse<SummaryTypes>(sum.ToString()) : SummaryTypes.Summary;
        var summethod = json["SummaryMethod"] is { } method ? Enum.Parse<SummaryMethods>(method.ToString()) : (sumtype == SummaryTypes.PageCount ? SummaryMethods.Page : SummaryMethods.Group);
        if (json["BreakKey"] is not null && summethod is not (SummaryMethods.Group or SummaryMethods.GroupIncremental)) throw new($"when SummaryElement is SummaryMethod={summethod}, BreakKey is invalid");
        return new SummaryElement()
        {
            X = posx,
            Y = posy,
            Name = name,
            Bind = json["Bind"]?.ToString() ?? "",
            Format = json["Format"]?.ToString() ?? "",
            Font = ToFontPathArray(json["Font"]),
            Size = (int)json.ToNode("Size").AsValue(),
            Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignments>(align.ToString()) : TextAlignments.Start,
            Style = json["Style"] is { } style ? Enum.Parse<TextStyles>(style.ToString()) : TextStyles.None,
            Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
            Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
            Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
            SummaryType = sumtype,
            SummaryMethod = summethod,
            BreakKey = json["BreakKey"]?.ToString() ?? "",
            Culture = json["Culture"] is { } ci ? CultureInfo.GetCultureInfo(ci.ToString()) : null,
        };
    }

    public static LineElement LoadLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
    };

    public static CrossSectionLineElement LoadCrossSectionLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
    };

    public static RectangleElement LoadRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
    };

    public static CrossSectionRectangleElement LoadCrossSectionRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
    };

    public static CrossSectionFillRectangleElement LoadCrossSectionFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        LineColor = ColorTranslator.FromHtml(json.ToNode("LineColor").ToString()),
        FillColor = ColorTranslator.FromHtml(json.ToNode("FillColor").ToString()),
        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
    };

    public static ImageElement LoadImageElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Path = json["Path"]?.ToString() ?? "",
        Bind = json["Bind"]?.ToString() ?? "",
        ZoomWidth = json["ZoomWidth"]?.AsValue() is { } zoomwidth ? (double)zoomwidth : 1.0,
        ZoomHeight = json["ZoomHeight"]?.AsValue() is { } zoomheight ? (double)zoomheight : 1.0,
    };

    public static FillRectangleElement LoadFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
        Height = json["Height"]?.AsValue() is { } height ? (int)height : 0,
        LineColor = ColorTranslator.FromHtml(json.ToNode("LineColor").ToString()),
        FillColor = ColorTranslator.FromHtml(json.ToNode("FillColor").ToString()),
        LineWidth = json["LineWidth"]?.AsValue() is { } linewidth ? (int)linewidth : 1,
    };

    public static FontPath[] ToFontPathArray(JsonNode? node) =>
        node is JsonValue v ? [new() { Path = (string)v! }] :
        node is JsonArray xs ? [.. xs.Select(x => ToFontPath(x!))] :
        [];

    public static FontPath ToFontPath(JsonNode node) =>
        node is JsonValue v ? new() { Path = (string)v! } :
        new()
        {
            Path = node.ToNode("Path").ToString(),
            Embed = node["Embed"] is { } embed ? Enum.Parse<FontEmbeds>(embed.ToString()) : FontEmbeds.PossibleEmbed | FontEmbeds.ConvertNone,
        };

    public static PageSize LoadPageSize(JsonNode? node)
    {
        if (node is JsonValue v) return PageSize.Parse((string)v!);
        else if (node is JsonArray xs)
        {
            switch (xs.Count)
            {
                case 2: return new((int)xs[0]!, (int)xs[1]!);
            }
        }
        else if (node is JsonNode n) return new((int)n.ToNode("Width"), (int)n.ToNode("Height"));
        return new(PageSizes.A4);
    }

    public static AllSides LoadAllSides(JsonNode? node)
    {
        if (node is JsonValue v) return new((int)v, (int)v, (int)v, (int)v);
        else if (node is JsonArray xs)
        {
            return xs.Count switch
            {
                0 => new(0, 0, 0, 0),
                1 => new((int)xs[0]!, (int)xs[0]!, (int)xs[0]!, (int)xs[0]!),
                2 => new((int)xs[0]!, (int)xs[0]!, (int)xs[1]!, (int)xs[1]!),
                3 => new((int)xs[0]!, (int)xs[2]!, (int)xs[1]!, (int)xs[1]!),
                _ => new((int)xs[0]!, (int)xs[2]!, (int)xs[3]!, (int)xs[1]!),
            };
        }
        else if (node is JsonNode n) return new((int)n.ToNode("Top"), (int)n.ToNode("Bottom"), (int)n.ToNode("Left"), (int)n.ToNode("Right"));
        return new(0, 0, 0, 0);
    }
}
