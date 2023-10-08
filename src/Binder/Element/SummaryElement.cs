﻿using System.Drawing;

namespace PicoPDF.Binder.Element;

public class SummaryElement : ITextElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Bind { get; set; }
    public string SummaryBind { get; set; } = "";
    public string Format { get; init; } = "";
    public required int Size { get; init; }
    public string Font { get; init; } = "";
    public TextAlignment Alignment { get; init; } = TextAlignment.Start;
    public TextStyle Style { get; init; } = TextStyle.None;
    public int Width { get; init; }
    public Color? Color { get; init; } = null;
    public SummaryType SummaryType { get; init; } = SummaryType.Summary;
    public SummaryMethod SummaryMethod { get; init; } = SummaryMethod.Increment;
    public string BreakKey { get; init; } = "";
    public bool Cliping { get; init; }
}
