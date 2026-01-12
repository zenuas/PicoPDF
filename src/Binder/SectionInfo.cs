using PicoPDF.Binder.Data;

namespace PicoPDF.Binder;

public record class SectionInfo(string BreakKey, string[] BreakKeyHierarchy, ISection Section, int Level);
