using Binder.Data;
using System;

namespace PicoPDF.Loader.Sections;

public class FooterSection : IFooterSection, ISectionStyle
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => field; init => field = value is ViewModes.Last or ViewModes.Every ? value : throw new ArgumentException(); } = ViewModes.Last;
    public IElement[] Elements { get; init; } = [];
    public bool IsFill { get => Style.HasFlag(SectionStyles.Fill); }
    public bool IsPageBreak { get => Style.HasFlag(SectionStyles.PageBreak); }
    public bool IsFooter { get; init; } = true;
    public SectionStyles Style { get; init; } = SectionStyles.None;
}
