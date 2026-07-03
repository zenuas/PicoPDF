using Binder.Data;
using System;

namespace PicoPDF.Loader.Sections;

public class HeaderSection : IHeaderSection, ISectionStyle
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get; init => field = value is ViewModes.First or ViewModes.Every or ViewModes.PageFirst ? value : throw new ArgumentException(); } = ViewModes.First;
    public IElement[] Elements { get; init; } = [];
    public SectionStyles Style { get; init => field = !value.HasFlag(SectionStyles.PageBreak | SectionStyles.Fill) ? value : throw new ArgumentException(); } = SectionStyles.None;
}
