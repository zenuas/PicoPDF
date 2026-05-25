using Binder.Model;
using System.Globalization;

namespace Binder.Data;

public interface IPageSection<TSection> : IParentSection
    where TSection : ISectionModel<TSection>
{
    public int Width { get; }
    public int Height { get; }
    public AllSides Padding { get; init; }
    public CultureInfo DefaultCulture { get; init; }
}
