using Extensions;
using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace PicoPDF.Section;

public class SectionLoader
{
    public static Section Load(string path)
    {
        var json = JsonNode.Parse(File.ReadAllText(path))!;

        var sections = json["Sections"]!
            .AsArray()
            .Select(x => LoadSubSection(x!))
            .ToDictionary(x => x.Name, x => x);

        return LoadSection(json["Page"]!, sections);
    }
    public static Section LoadSection(JsonNode json, Dictionary<string, ISection> sections)
    {
        var header = json["Header"] is { } p1 ? sections[p1.ToString()].Cast<IHeaderSection>() : null;
        var footer = json["Footer"] is { } p2 ? sections[p2.ToString()].Cast<IFooterSection>() : null;
        var detail = json["Detail"] is JsonObject o ? LoadSection(o, sections) : sections[json["Detail"]!.ToString()].Cast<ISubSection>();

        return new Section() { SubSection = detail, Header = header, Footer = footer };
    }

    public static ISection LoadSubSection(JsonNode json)
    {
        var elements = json["Elements"]!.AsArray().Select(x => LoadElement(x!)).ToList();
        var name = json["Name"]!.ToString();
        var height = (int)json["Height"]!.AsValue();
        switch (json["Type"]!.ToString())
        {
            case "HeaderSection":
                return new HeaderSection() { Name = name, Height = height, Elements = elements };

            case "DetailSection":
                return new DetailSection() { Name = name, Height = height, Elements = elements };

            case "TotalSection":
                return new TotalSection() { Name = name, Height = height, Elements = elements };

            case "FooterSection":
                return new FooterSection() { Name = name, Height = height, Elements = elements };
        }
        throw new Exception();
    }

    public static IElement LoadElement(JsonNode json)
    {
        var posx = (int)json["X"]!.AsValue();
        var posy = (int)json["Y"]!.AsValue();
        switch (json["Type"]!.ToString())
        {
            case "TextElement":
                return new TextElement() { X = posx, Y = posy, Text = json["Text"]!.ToString() };

            case "BindElement":
                return new BindElement() { X = posx, Y = posy, Bind = json["Bind"]!.ToString(), Format = json["Format"]?.ToString() ?? "" };
        }
        throw new Exception();
    }
}
