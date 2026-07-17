using Binder.Data;
using Mina.Extension;
using Pdf.Documents;
using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PicoPDF.Loader;

public static class JsonLoader
{
    public static PageSection CreatePageFromJsonFile(string path, PdfEventOption option) => CreatePageFromJson(File.ReadAllText(path), option);

    public static PageSection CreatePageFromJson(string json, PdfEventOption option) => CreatePageFromJson(JsonNode.Parse(json, null, new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })!, option);

    public static PageSection CreatePageFromJson(JsonNode json, PdfEventOption option)
    {
        var fonts = ToFontPathArray(json["DefaultFont"]);
        if (fonts.Length == 0) throw new("DefaultFont is empty");

        var size = LoadPageSize(json["Size"]);
        var padding = LoadAllSides(json["Padding"]);
        var sections = json.GetNode("Sections")
            .AsArray()
            .Select(x => LoadSubSection(x!, size.Width - padding.Left - padding.Right))
            .ToDictionary(x => x.Name, x => x);

        return new()
        {
            Size = size,
            Orientation = json.GetEnumOrDefaultWithoutNullValue<Orientations>("Orientation") ?? Orientations.Vertical,
            DefaultFont = fonts,
            Header = json.GetStringOrDefaultWithoutNullValue("Header") is { } p1 ? sections[p1].Cast<IHeaderSection>() : null,
            Footer = json.GetStringOrDefaultWithoutNullValue("Footer") is { } p2 ? sections[p2].Cast<IFooterSection>() : null,
            SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.GetStringValue("Detail")].Cast<ISubSection>(),
            Padding = padding,
            DefaultCulture = json.GetCultureOrDefaultWithoutNullValue("DefaultCulture") ?? CultureInfo.InvariantCulture,
            EventOption = option,
        };
    }

    public static Section LoadSection(JsonNode json, Dictionary<string, ISection> sections) => new()
    {
        BreakKey = json.GetStringOrDefaultWithoutNullValue("BreakKey") ?? "",
        Header = json.GetStringOrDefaultWithoutNullValue("Header") is { } p1 ? sections[p1].Cast<IHeaderSection>() : null,
        Footer = json.GetStringOrDefaultWithoutNullValue("Footer") is { } p2 ? sections[p2].Cast<IFooterSection>() : null,
        SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.GetStringValue("Detail")].Cast<ISubSection>(),
    };

    public static ISection LoadSubSection(JsonNode json, int width)
    {
        var name = json.GetStringValue("Name");
        var height = json.GetIntValue("Height");
        var elements = json.GetNode("Elements").AsArray().Select(x => LoadElement(x!)).ToArray();
        var viewmode = json.GetEnumOrDefaultWithoutNullValue<ViewModes>("ViewMode") ?? ViewModes.Every;
        var style = json.GetEnumOrDefaultWithoutNullValue<SectionStyles>("Style") ?? SectionStyles.None;
        return json.GetStringValue("Type") switch
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
        var posx = json.GetIntValue("X");
        var posy = json.GetIntValue("Y");
        var name = json.GetStringOrDefaultWithoutNullValue("Name") ?? "";
        return json.GetStringValue("Type") switch
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
        Text = json.GetStringValue("Text"),
        Font = ToFontPathArray(json["Font"]),
        Size = json.GetIntValue("Size"),
        Alignment = json.GetEnumOrDefaultWithoutNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
        Style = json.GetEnumOrDefaultWithoutNullValue<TextStyles>("Style") ?? TextStyles.None,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        Color = json.GetColorOrDefaultWithoutNullValue("Color"),
    };

    public static BindElement LoadBindElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Bind = json.GetNode("Bind").ToString(),
        Format = json.GetStringOrDefaultWithoutNullValue("Format") ?? "",
        Font = ToFontPathArray(json["Font"]),
        Size = json.GetIntValue("Size"),
        Alignment = json.GetEnumOrDefaultWithoutNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
        Style = json.GetEnumOrDefaultWithoutNullValue<TextStyles>("Style") ?? TextStyles.None,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        Color = json.GetColorOrDefaultWithoutNullValue("Color"),
        Culture = json.GetCultureOrDefaultWithoutNullValue("Culture"),
    };

    public static SummaryElement LoadSummaryElement(int posx, int posy, string name, JsonNode json)
    {
        var sumtype = json.GetEnumOrDefaultWithoutNullValue<SummaryTypes>("SummaryType") ?? SummaryTypes.Summary;
        var summethod = json.GetEnumOrDefaultWithoutNullValue<SummaryMethods>("SummaryMethod") ?? (sumtype == SummaryTypes.PageCount ? SummaryMethods.Page : SummaryMethods.Group);
        if (json["BreakKey"] is not null && summethod is not (SummaryMethods.Group or SummaryMethods.GroupIncremental)) throw new($"when SummaryElement is SummaryMethod={summethod}, BreakKey is invalid");
        return new SummaryElement()
        {
            X = posx,
            Y = posy,
            Name = name,
            Bind = json.GetStringOrDefaultWithoutNullValue("Bind") ?? "",
            Format = json.GetStringOrDefaultWithoutNullValue("Format") ?? "",
            Font = ToFontPathArray(json["Font"]),
            Size = json.GetIntValue("Size"),
            Alignment = json.GetEnumOrDefaultWithoutNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
            Style = json.GetEnumOrDefaultWithoutNullValue<TextStyles>("Style") ?? TextStyles.None,
            Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
            Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
            Color = json.GetColorOrDefaultWithoutNullValue("Color"),
            SummaryType = sumtype,
            SummaryMethod = summethod,
            BreakKey = json.GetStringOrDefaultWithoutNullValue("BreakKey") ?? "",
            Culture = json.GetCultureOrDefaultWithoutNullValue("Culture"),
        };
    }

    public static LineElement LoadLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        Color = json.GetColorOrDefaultWithoutNullValue("Color"),
        LineWidth = json.GetIntOrDefaultWithoutNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionLineElement LoadCrossSectionLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        Color = json.GetColorOrDefaultWithoutNullValue("Color"),
        LineWidth = json.GetIntOrDefaultWithoutNullValue("LineWidth") ?? 1,
    };

    public static RectangleElement LoadRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        Color = json.GetColorOrDefaultWithoutNullValue("Color"),
        LineWidth = json.GetIntOrDefaultWithoutNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionRectangleElement LoadCrossSectionRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        Color = json.GetColorOrDefaultWithoutNullValue("Color"),
        LineWidth = json.GetIntOrDefaultWithoutNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionFillRectangleElement LoadCrossSectionFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        LineColor = json.GetColorValue("LineColor"),
        FillColor = json.GetColorValue("FillColor"),
        LineWidth = json.GetIntOrDefaultWithoutNullValue("LineWidth") ?? 1,
    };

    public static ImageElement LoadImageElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Path = json.GetStringOrDefaultWithoutNullValue("Path") ?? "",
        Bind = json.GetStringOrDefaultWithoutNullValue("Bind") ?? "",
        ZoomWidth = json.GetDoubleOrDefaultWithoutNullValue("ZoomWidth") ?? 1.0,
        ZoomHeight = json.GetDoubleOrDefaultWithoutNullValue("ZoomHeight") ?? 1.0,
    };

    public static FillRectangleElement LoadFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrDefaultWithoutNullValue("Width") ?? 0,
        Height = json.GetIntOrDefaultWithoutNullValue("Height") ?? 0,
        LineColor = json.GetColorValue("LineColor"),
        FillColor = json.GetColorValue("FillColor"),
        LineWidth = json.GetIntOrDefaultWithoutNullValue("LineWidth") ?? 1,
    };

    public static FontPath[] ToFontPathArray(JsonNode? node) =>
        node is JsonArray xs ? [.. xs.Select(x => ToFontPath(x!))] :
        node is { } ? [ToFontPath(node)] :
        [];

    public static FontPath ToFontPath(JsonNode node) =>
        node is JsonValue v ? new() { Path = (string)v! } :
        new()
        {
            Path = node.GetStringValue("Path"),
            Embed = node.GetEnumOrDefaultWithoutNullValue<FontEmbeds>("Embed") ?? (FontEmbeds.PossibleEmbed | FontEmbeds.ConvertNone),
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
        else if (node is JsonNode n) return new(n.GetIntValue("Width"), n.GetIntValue("Height"));
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
        else if (node is JsonNode n) return new(n.GetIntValue("Top"), n.GetIntValue("Bottom"), n.GetIntValue("Left"), n.GetIntValue("Right"));
        return new(0, 0, 0, 0);
    }
}
