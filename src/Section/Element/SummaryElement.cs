﻿namespace PicoPDF.Section.Element;

public class SummaryElement : ISectionElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Bind { get; set; }
    public string CountBind { get; set; } = "";
    public string Format { get; init; } = "";
    public required int Size { get; init; }
    public string Font { get; init; } = "";
    public SummaryType SummaryType { get; init; } = SummaryType.Summary;
}