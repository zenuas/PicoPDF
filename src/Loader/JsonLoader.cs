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
    public static JsonNode GetNode(this JsonNode self, string name) => self[name] ?? throw new NullReferenceException($"Element '{name}' was not found.");
    public static JsonValue GetValue(this JsonNode self, string name) => self.GetNode(name).AsValue();

    public static int GetIntValue(this JsonNode self, string name) => (int)self.GetValue(name);
    public static double GetDoubleValue(this JsonNode self, string name) => (double)self.GetValue(name);
    public static bool GetBoolValue(this JsonNode self, string name) => (bool)self.GetValue(name);
    public static string GetStringValue(this JsonNode self, string name) => self.GetValue(name).ToString();
    public static T GetEnumValue<T>(this JsonNode self, string name) where T : struct, Enum => Enum.Parse<T>(self.GetStringValue(name));
    public static Color GetColorValue(this JsonNode self, string name) => ColorTranslator.FromHtml(self.GetStringValue(name));
    public static CultureInfo GetCultureValue(this JsonNode self, string name) => CultureInfo.GetCultureInfo(self.GetStringValue(name));

    public static int? GetIntOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (int)x.AsValue() : null;
    public static double? GetDoubleOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (double)x.AsValue() : null;
    public static bool? GetBoolOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (bool)x.AsValue() : null;
    public static string? GetStringOrNullValue(this JsonNode self, string name) => self[name] is { } x ? x.AsValue().ToString() : null;
    public static T? GetEnumOrNullValue<T>(this JsonNode self, string name) where T : struct, Enum => self.GetStringOrNullValue(name) is { } x ? Enum.Parse<T>(x) : null;
    public static Color? GetColorOrNullValue(this JsonNode self, string name) => self.GetStringOrNullValue(name) is { } x ? ColorTranslator.FromHtml(x) : null;
    public static CultureInfo? GetCultureOrNullValue(this JsonNode self, string name) => self.GetStringOrNullValue(name) is { } x ? CultureInfo.GetCultureInfo(x) : null;

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
            Orientation = json.GetEnumOrNullValue<Orientations>("Orientation") ?? Orientations.Vertical,
            DefaultFont = fonts,
            Header = json.GetStringOrNullValue("Header") is { } p1 ? sections[p1].Cast<IHeaderSection>() : null,
            Footer = json.GetStringOrNullValue("Footer") is { } p2 ? sections[p2].Cast<IFooterSection>() : null,
            SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.GetStringValue("Detail").ToString()].Cast<ISubSection>(),
            Padding = padding,
            DefaultCulture = json.GetCultureOrNullValue("DefaultCulture") ?? CultureInfo.InvariantCulture,
            EventOption = option,
        };
    }

    public static Section LoadSection(JsonNode json, Dictionary<string, ISection> sections) => new()
    {
        BreakKey = json.GetStringOrNullValue("BreakKey") ?? "",
        Header = json.GetStringOrNullValue("Header") is { } p1 ? sections[p1].Cast<IHeaderSection>() : null,
        Footer = json.GetStringOrNullValue("Footer") is { } p2 ? sections[p2].Cast<IFooterSection>() : null,
        SubSection = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json.GetStringValue("Detail")].Cast<ISubSection>(),
    };

    public static ISection LoadSubSection(JsonNode json, int width)
    {
        var name = json.GetStringValue("Name");
        var height = json.GetIntValue("Height");
        var elements = json.GetNode("Elements").AsArray().Select(x => LoadElement(x!)).ToArray();
        var viewmode = json.GetEnumOrNullValue<ViewModes>("ViewMode") ?? ViewModes.Every;
        var style = json.GetEnumOrNullValue<SectionStyles>("Style") ?? SectionStyles.None;
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
        var name = json.GetStringOrNullValue("Name") ?? "";
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
        Alignment = json.GetEnumOrNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
        Style = json.GetEnumOrNullValue<TextStyles>("Style") ?? TextStyles.None,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        Color = json.GetColorOrNullValue("Color"),
    };

    public static BindElement LoadBindElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Bind = json.GetNode("Bind").ToString(),
        Format = json.GetStringOrNullValue("Format") ?? "",
        Font = ToFontPathArray(json["Font"]),
        Size = json.GetIntValue("Size"),
        Alignment = json.GetEnumOrNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
        Style = json.GetEnumOrNullValue<TextStyles>("Style") ?? TextStyles.None,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        Color = json.GetColorOrNullValue("Color"),
        Culture = json.GetCultureOrNullValue("Culture"),
    };

    public static SummaryElement LoadSummaryElement(int posx, int posy, string name, JsonNode json)
    {
        var sumtype = json.GetEnumOrNullValue<SummaryTypes>("SummaryType") ?? SummaryTypes.Summary;
        var summethod = json.GetEnumOrNullValue<SummaryMethods>("SummaryMethod") ?? (sumtype == SummaryTypes.PageCount ? SummaryMethods.Page : SummaryMethods.Group);
        if (json["BreakKey"] is not null && summethod is not (SummaryMethods.Group or SummaryMethods.GroupIncremental)) throw new($"when SummaryElement is SummaryMethod={summethod}, BreakKey is invalid");
        return new SummaryElement()
        {
            X = posx,
            Y = posy,
            Name = name,
            Bind = json.GetStringOrNullValue("Bind") ?? "",
            Format = json.GetStringOrNullValue("Format") ?? "",
            Font = ToFontPathArray(json["Font"]),
            Size = json.GetIntValue("Size"),
            Alignment = json.GetEnumOrNullValue<TextAlignments>("Alignment") ?? TextAlignments.Start,
            Style = json.GetEnumOrNullValue<TextStyles>("Style") ?? TextStyles.None,
            Width = json.GetIntOrNullValue("Width") ?? 0,
            Height = json.GetIntOrNullValue("Height") ?? 0,
            Color = json.GetColorOrNullValue("Color"),
            SummaryType = sumtype,
            SummaryMethod = summethod,
            BreakKey = json.GetStringOrNullValue("BreakKey") ?? "",
            Culture = json.GetCultureOrNullValue("Culture"),
        };
    }

    public static LineElement LoadLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        Color = json.GetColorOrNullValue("Color"),
        LineWidth = json.GetIntOrNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionLineElement LoadCrossSectionLineElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        Color = json.GetColorOrNullValue("Color"),
        LineWidth = json.GetIntOrNullValue("LineWidth") ?? 1,
    };

    public static RectangleElement LoadRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        Color = json.GetColorOrNullValue("Color"),
        LineWidth = json.GetIntOrNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionRectangleElement LoadCrossSectionRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        Color = json.GetColorOrNullValue("Color"),
        LineWidth = json.GetIntOrNullValue("LineWidth") ?? 1,
    };

    public static CrossSectionFillRectangleElement LoadCrossSectionFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        LineColor = json.GetColorValue("LineColor"),
        FillColor = json.GetColorValue("FillColor"),
        LineWidth = json.GetIntOrNullValue("LineWidth") ?? 1,
    };

    public static ImageElement LoadImageElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Path = json.GetStringOrNullValue("Path") ?? "",
        Bind = json.GetStringOrNullValue("Bind") ?? "",
        ZoomWidth = json.GetDoubleOrNullValue("ZoomWidth") ?? 1.0,
        ZoomHeight = json.GetDoubleOrNullValue("ZoomHeight") ?? 1.0,
    };

    public static FillRectangleElement LoadFillRectangleElement(int posx, int posy, string name, JsonNode json) => new()
    {
        X = posx,
        Y = posy,
        Name = name,
        Width = json.GetIntOrNullValue("Width") ?? 0,
        Height = json.GetIntOrNullValue("Height") ?? 0,
        LineColor = json.GetColorValue("LineColor"),
        FillColor = json.GetColorValue("FillColor"),
        LineWidth = json.GetIntOrNullValue("LineWidth") ?? 1,
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
            Embed = node.GetEnumOrNullValue<FontEmbeds>("Embed") ?? (FontEmbeds.PossibleEmbed | FontEmbeds.ConvertNone),
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
