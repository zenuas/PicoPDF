using Binder.Data;

namespace Binder;

public record class SectionInfo(string BreakKey, int BreakCount, ISection Section, int Depth);
