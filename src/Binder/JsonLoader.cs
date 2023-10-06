﻿using Extensions;
using PicoPDF.Binder.Data;
using PicoPDF.Binder.Element;
using PicoPDF.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PicoPDF.Binder;

public static class JsonLoader
{
    public static PageSection Load(string path)
    {
        var json = JsonNode.Parse(
                File.ReadAllText(path),
                null,
                new JsonDocumentOptions() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip }
            )!;

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
        switch (json["Type"]!.ToString())
        {
            case "HeaderSection":
                return new HeaderSection() { Name = name, Height = height, ViewMode = viewmode, Elements = elements };

            case "DetailSection":
                return new DetailSection() { Name = name, Height = height, Elements = elements };

            case "TotalSection":
                return new TotalSection() { Name = name, Height = height, ViewMode = viewmode, Elements = elements, PageBreak = pagebreak };

            case "FooterSection":
                return new FooterSection() { Name = name, Height = height, ViewMode = viewmode, Elements = elements, PageBreak = pagebreak };
        }
        throw new();
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
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
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
                        SummaryType = sumtype,
                        SummaryMethod = json["SummaryMethod"] is { } method ? Enum.Parse<SummaryMethod>(method.ToString()) : (sumtype == SummaryType.PageCount ? SummaryMethod.Increment : SummaryMethod.Group),
                        BreakKey = json["BreakKey"]?.ToString() ?? "",
                        Alignment = json["Alignment"] is { } align ? Enum.Parse<TextAlignment>(align.ToString()) : TextAlignment.Start,
                        Width = json["Width"]?.AsValue() is { } width ? (int)width : 0,
                        Color = json["Color"]?.ToString() is { } color ? ColorTranslator.FromHtml(color) : null,
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
}
