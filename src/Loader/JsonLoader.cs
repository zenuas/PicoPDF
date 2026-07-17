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
    public static JsonValue ToValue(this JsonNode self, string name) => self.ToNode(name).AsValue();

    public static int ToIntValue(this JsonNode self, string name) => (int)self.ToValue(name);
    public static double ToDoubleValue(this JsonNode self, string name) => (double)self.ToValue(name);
    public static bool ToBoolValue(this JsonNode self, string name) => (bool)self.ToValue(name);
    public static string ToStringValue(this JsonNode self, string name) => self.ToValue(name).ToString();
    public static T ToEnumValue<T>(this JsonNode self, string name) where T : struct, Enum => Enum.Parse<T>(self.ToStringValue(name));
    public static Color ToColorValue(this JsonNode self, string name) => ColorTranslator.FromHtml(self.ToStringValue(name));
    public static CultureInfo ToCultureValue(this JsonNode self, string name) => CultureInfo.GetCultureInfo(self.ToStringValue(name));

    public static int? ToIntOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (int)x.AsValue() : null;
    public static double? ToDoubleOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (double)x.AsValue() : null;
    public static bool? ToBoolOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (bool)x.AsValue() : null;
    public static string? ToStringOrNullValue(this JsonNode self, string name) => self[name] is { } x ? x.AsValue().ToString() : null;
    public static T? ToEnumOrNullValue<T>(this JsonNode self, string name) where T : struct, Enum => self.ToStringOrNullValue(name) is { } x ? Enum.Parse<T>(x) : null;
    public static Color? ToColorOrNullValue(this JsonNode self, string name) => self.ToStringOrNullValue(name) is { } x ? ColorTranslator.FromHtml(x) : null;
    public static CultureInfo? ToCultureOrNullValue(this JsonNode self, string name) => self.ToStringOrNullValue(name) is { } x ? CultureInfo.GetCultureInfo(x) : null;

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
            Orientation = json["Orientation"]?.AsValue() is { } orient ? Enum.Parse<Orientations>(orient!.ToString()) : Orientations.Vertical,
            DefaultFont = fonts,
            Header = json.ToStringOrNullValue("Header") is { } p1 ? sections[p1].Cast<IHeaderSection>() : null,
            Footer = json.ToStringOrNullValue("Footer") is { } p2 ? sections[p2].Cast<IFooterSection>() : null,
            SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.ToStringValue("Detail").ToString()].Cast<ISubSection>(),
            Padding = padding,
            DefaultCulture = json.ToCultureOrNullValue("DefaultCulture") ?? CultureInfo.InvariantCulture,
            EventOption = option,
        };
    }

    public static Section LoadSection(JsonNode json, Dictionary<string, ISection> sections) => new()
    {
        BreakKey = json.ToStringOrNullValue("BreakKey") ?? "",
        Header = json.ToStringOrNullValue("Header") is { } p1 ? sections[p1].Cast<IHeaderSection>() : null,
        Footer = json.ToStringOrNullValue("Footer") is { } p2 ? sections[p2].Cast<IFooterSection>() : null,
        SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.ToStringValue("Detail")].Cast<ISubSection>(),
    };

    public static ISection LoadSubSection(JsonNode json, int width)
    {
        var name = json.ToStringValue("Name");
        var height = json.ToIntValue("Height");
        var elements = json.ToNode("Elements").AsArray().Select(x => LoadElement(x!)).ToArray();
        var viewmode = json.ToEnumOrNullValue<ViewModes>("ViewMode") ?? ViewModes.Every;
        var style = json.ToEnumOrNullValue<SectionStyles>("Style") ?? SectionStyles.None;
        return json.ToStringValue("Type") switch
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
        var posx = json.ToIntValue("X");
        var posy = json.ToIntValue("Y");
        var name = json.ToStringOrNullValue("Name") ?? "";
        return json.ToStringValue("Type") switch
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
        Text = json.ToStringValue("Text"),
        Font = ToFontPathArray(json["Font"]),
        Size = json.ToIntValue("Size"),
        Alignment = json.ToEnumOrNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
        Style = json.ToEnumOrNullValue<TextStyles>("Style") ?? TextStyles.None,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        Color = json.ToColorOrNullValue("Color"),
    };

    public static BindElement LoadBindElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Bind = json.ToNode("Bind").ToString(),
        Format = json.ToStringOrNullValue("Format") ?? "",
        Font = ToFontPathArray(json["Font"]),
        Size = json.ToIntValue("Size"),
        Alignment = json.ToEnumOrNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
        Style = json.ToEnumOrNullValue<TextStyles>("Style") ?? TextStyles.None,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        Color = json.ToColorOrNullValue("Color"),
        Culture = json.ToCultureOrNullValue("Culture"),
    };

    public static SummaryElement LoadSummaryElement(int posx, int posy, string name, JsonNode json)
    {
        var sumtype = json.ToEnumOrNullValue<SummaryTypes>("SummaryType") ?? SummaryTypes.Summary;
        var summethod = json.ToEnumOrNullValue<SummaryMethods>("SummaryMethod") ?? (sumtype == SummaryTypes.PageCount ? SummaryMethods.Page : SummaryMethods.Group);
        if (json["BreakKey"] is not null && summethod is not (SummaryMethods.Group or SummaryMethods.GroupIncremental)) throw new($"when SummaryElement is SummaryMethod={summethod}, BreakKey is invalid");
        return new SummaryElement()
        {
            X = posx,
            Y = posy,
            Name = name,
            Bind = json.ToStringOrNullValue("Bind") ?? "",
            Format = json.ToStringOrNullValue("Format") ?? "",
            Font = ToFontPathArray(json["Font"]),
            Size = json.ToIntValue("Size"),
            Alignment = json.ToEnumOrNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
            Style = json.ToEnumOrNullValue<TextStyles>("Style") ?? TextStyles.None,
            Width = json.ToIntOrNullValue("Width") ?? 0,
            Height = json.ToIntOrNullValue("Height") ?? 0,
            Color = json.ToColorOrNullValue("Color"),
            SummaryType = sumtype,
            SummaryMethod = summethod,
            BreakKey = json.ToStringOrNullValue("BreakKey") ?? "",
            Culture = json.ToCultureOrNullValue("Culture"),
        };
    }

    public static LineElement LoadLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        Color = json.ToColorOrNullValue("Color"),
        LineWidth = json.ToIntOrNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionLineElement LoadCrossSectionLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        Color = json.ToColorOrNullValue("Color"),
        LineWidth = json.ToIntOrNullValue("LineWidth") ?? 1,
    };

    public static RectangleElement LoadRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        Color = json.ToColorOrNullValue("Color"),
        LineWidth = json.ToIntOrNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionRectangleElement LoadCrossSectionRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        Color = json.ToColorOrNullValue("Color"),
        LineWidth = json.ToIntOrNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionFillRectangleElement LoadCrossSectionFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        LineColor = json.ToColorValue("LineColor"),
        FillColor = json.ToColorValue("FillColor"),
        LineWidth = json.ToIntOrNullValue("LineWidth") ?? 1,
    };

    public static ImageElement LoadImageElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Path = json.ToStringOrNullValue("Path") ?? "",
        Bind = json.ToStringOrNullValue("Bind") ?? "",
        ZoomWidth = json.ToDoubleOrNullValue("ZoomWidth") ?? 1.0,
        ZoomHeight = json.ToDoubleOrNullValue("ZoomHeight") ?? 1.0,
    };

    public static FillRectangleElement LoadFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.ToIntOrNullValue("Width") ?? 0,
        Height = json.ToIntOrNullValue("Height") ?? 0,
        LineColor = json.ToColorValue("LineColor"),
        FillColor = json.ToColorValue("FillColor"),
        LineWidth = json.ToIntOrNullValue("LineWidth") ?? 1,
    };

    public static FontPath[] ToFontPathArray(JsonNode? node) =>
        node is JsonArray xs ? [.. xs.Select(x => ToFontPath(x!))] :
        node is { } ? [ToFontPath(node)] :
        [];

    public static FontPath ToFontPath(JsonNode node) =>
        node is JsonValue v ? new() { Path = (string)v! } :
        new()
        {
            Path = node.ToStringValue("Path"),
            Embed = node.ToEnumOrNullValue<FontEmbeds>("Embed") ?? (FontEmbeds.PossibleEmbed | FontEmbeds.ConvertNone),
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
        else if (node is JsonNode n) return new(n.ToIntValue("Width"), n.ToIntValue("Height"));
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
        else if (node is JsonNode n) return new(n.ToIntValue("Top"), n.ToIntValue("Bottom"), n.ToIntValue("Left"), n.ToIntValue("Right"));
        return new(0, 0, 0, 0);
    }
}
