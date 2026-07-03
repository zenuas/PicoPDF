using Binder.Data;
using System;

namespace PicoPDF.Loader.Sections;

public class TotalSection : IFooterSection, ISectionStyle
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get; init => field = value is ViewModes.Last or ViewModes.Every ? value : throw new ArgumentException(); } = ViewModes.Last;
    public IElement[] Elements { get; init; } = [];
    public bool IsFill => Style.HasFlag(SectionStyles.Fill);
    public bool IsPageBreak => Style.HasFlag(SectionStyles.PageBreak);
    public bool IsFooter => false;
    public bool IsHeightAdjusting => Style.HasFlag(SectionStyles.HeightAdjusting);
    public SectionStyles Style { get; init; } = SectionStyles.None;
}
