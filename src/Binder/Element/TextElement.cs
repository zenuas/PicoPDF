﻿namespace PicoPDF.Binder.Element;

public class TextElement : IElement
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Text { get; init; }
    public required int Size { get; init; }
    public string Font { get; init; } = "";
}