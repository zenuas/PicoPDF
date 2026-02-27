using Binder.Data;

namespace Binder;

public record class SectionInfo(string BreakKey, string[] BreakKeyHierarchy, ISection Section, int Depth);
