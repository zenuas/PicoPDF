using Binder.Data;
using System;

namespace PicoPDF.Loader.Sections;

public class DetailSection : IDetailSection, ISubSection, ISectionStyle
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewModes.Every; init => _ = value == ViewModes.Every ? value : throw new ArgumentException(); }
    public IElement[] Elements { get; init; } = [];
    public bool IsFill => Style.HasFlag(SectionStyles.Fill);
    public SectionStyles Style { get; init => field = !value.HasFlag(SectionStyles.PageBreak) ? value : throw new ArgumentException(); } = SectionStyles.None;
}
