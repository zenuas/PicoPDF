using OpenType;
using System;

namespace PicoPDF.TestAll;

public static class FontDump
{
    public static void Dump(IOpenTypeRequiredTables font, Option opt)
    {
        var head = font.FontHeader;
        Console.WriteLine($"head,MajorVersion,{head.MajorVersion}");
        Console.WriteLine($"head,MinorVersion,{head.MinorVersion}");
        Console.WriteLine($"head,FontRevision,{head.FontRevision}");
        Console.WriteLine($"head,ChecksumAdjustment,{head.ChecksumAdjustment}");
        Console.WriteLine($"head,MagicNumber,{head.MagicNumber}");
        Console.WriteLine($"head,Flags,{head.Flags}");
        Console.WriteLine($"head,UnitsPerEm,{head.UnitsPerEm}");
        Console.WriteLine($"head,Created,{head.Created}");
        Console.WriteLine($"head,Modified,{head.Modified}");
        Console.WriteLine($"head,XMin,{head.XMin}");
        Console.WriteLine($"head,YMin,{head.YMin}");
        Console.WriteLine($"head,XMax,{head.XMax}");
        Console.WriteLine($"head,YMax,{head.YMax}");
        Console.WriteLine($"head,MacStyle,{head.MacStyle}");
        Console.WriteLine($"head,LowestRecPPEM,{head.LowestRecPPEM}");
        Console.WriteLine($"head,FontDirectionHint,{head.FontDirectionHint}");
        Console.WriteLine($"head,IndexToLocFormat,{head.IndexToLocFormat}");
        Console.WriteLine($"head,GlyphDataFormat,{head.GlyphDataFormat}");

        var maxp = font.MaximumProfile;
        Console.WriteLine($"maxp,Version,{maxp.Version}");
        Console.WriteLine($"maxp,NumberOfGlyphs,{maxp.NumberOfGlyphs}");
        Console.WriteLine($"maxp,MaxPoints,{maxp.MaxPoints}");
        Console.WriteLine($"maxp,MaxContours,{maxp.MaxContours}");
        Console.WriteLine($"maxp,MaxCompositePoints,{maxp.MaxCompositePoints}");
        Console.WriteLine($"maxp,MaxCompositeContours,{maxp.MaxCompositeContours}");
        Console.WriteLine($"maxp,MaxZones,{maxp.MaxZones}");
        Console.WriteLine($"maxp,MaxTwilightPoints,{maxp.MaxTwilightPoints}");
        Console.WriteLine($"maxp,MaxStorage,{maxp.MaxStorage}");
        Console.WriteLine($"maxp,MaxFunctionDefs,{maxp.MaxFunctionDefs}");
        Console.WriteLine($"maxp,MaxInstructionDefs,{maxp.MaxInstructionDefs}");
        Console.WriteLine($"maxp,MaxStackElements,{maxp.MaxStackElements}");
        Console.WriteLine($"maxp,MaxSizeOfInstructions,{maxp.MaxSizeOfInstructions}");
        Console.WriteLine($"maxp,MaxComponentElements,{maxp.MaxComponentElements}");
        Console.WriteLine($"maxp,MaxComponentDepth,{maxp.MaxComponentDepth}");

        var post = font.PostScript;
        Console.WriteLine($"post,Version,{post.Version}");
        Console.WriteLine($"post,ItalicAngle,{post.ItalicAngle}");
        Console.WriteLine($"post,UnderlinePosition,{post.UnderlinePosition}");
        Console.WriteLine($"post,UnderlineThickness,{post.UnderlineThickness}");
        Console.WriteLine($"post,IsFixedPitch,{post.IsFixedPitch}");
        Console.WriteLine($"post,MinMemType42,{post.MinMemType42}");
        Console.WriteLine($"post,MaxMemType42,{post.MaxMemType42}");
        Console.WriteLine($"post,MinMemType1,{post.MinMemType1}");
        Console.WriteLine($"post,MaxMemType1,{post.MaxMemType1}");

        if (font.OS2 is { } os2)
        {
            Console.WriteLine($"os2,Version,{os2.Version}");
            Console.WriteLine($"os2,XAvgCharWidth,{os2.XAvgCharWidth}");
            Console.WriteLine($"os2,UsWeightClass,{os2.UsWeightClass}");
            Console.WriteLine($"os2,UsWidthClass,{os2.UsWidthClass}");
            Console.WriteLine($"os2,FsType,{os2.FsType}");
            Console.WriteLine($"os2,YSubscriptXSize,{os2.YSubscriptXSize}");
            Console.WriteLine($"os2,YSubscriptYSize,{os2.YSubscriptYSize}");
            Console.WriteLine($"os2,YSubscriptXOffset,{os2.YSubscriptXOffset}");
            Console.WriteLine($"os2,YSubscriptYOffset,{os2.YSubscriptYOffset}");
            Console.WriteLine($"os2,YSuperscriptXSize,{os2.YSuperscriptXSize}");
            Console.WriteLine($"os2,YSuperscriptYSize,{os2.YSuperscriptYSize}");
            Console.WriteLine($"os2,YSuperscriptXOffset,{os2.YSuperscriptXOffset}");
            Console.WriteLine($"os2,YSuperscriptYOffset,{os2.YSuperscriptYOffset}");
            Console.WriteLine($"os2,YStrikeoutSize,{os2.YStrikeoutSize}");
            Console.WriteLine($"os2,YStrikeoutPosition,{os2.YStrikeoutPosition}");
            Console.WriteLine($"os2,SFamilyClass,{os2.SFamilyClass}");
            Console.WriteLine($"os2,Panose,{os2.Panose}");
            Console.WriteLine($"os2,UlUnicodeRange1,{os2.UlUnicodeRange1}");
            Console.WriteLine($"os2,UlUnicodeRange2,{os2.UlUnicodeRange2}");
            Console.WriteLine($"os2,UlUnicodeRange3,{os2.UlUnicodeRange3}");
            Console.WriteLine($"os2,UlUnicodeRange4,{os2.UlUnicodeRange4}");
            Console.WriteLine($"os2,AchVendID,{os2.AchVendID}");
            Console.WriteLine($"os2,FsSelection,{os2.FsSelection}");
            Console.WriteLine($"os2,UsFirstCharIndex,{os2.UsFirstCharIndex}");
            Console.WriteLine($"os2,UsLastCharIndex,{os2.UsLastCharIndex}");
            Console.WriteLine($"os2,STypoAscender,{os2.STypoAscender}");
            Console.WriteLine($"os2,STypoDescender,{os2.STypoDescender}");
            Console.WriteLine($"os2,STypoLineGap,{os2.STypoLineGap}");
            Console.WriteLine($"os2,UsWinAscent,{os2.UsWinAscent}");
            Console.WriteLine($"os2,UsWinDescent,{os2.UsWinDescent}");
            Console.WriteLine($"os2,UlCodePageRange1,{os2.UlCodePageRange1}");
            Console.WriteLine($"os2,UlCodePageRange2,{os2.UlCodePageRange2}");
            Console.WriteLine($"os2,SxHeight,{os2.SxHeight}");
            Console.WriteLine($"os2,SCapHeight,{os2.SCapHeight}");
            Console.WriteLine($"os2,UsDefaultChar,{os2.UsDefaultChar}");
            Console.WriteLine($"os2,UsBreakChar,{os2.UsBreakChar}");
            Console.WriteLine($"os2,UsMaxContext,{os2.UsMaxContext}");
            Console.WriteLine($"os2,UsLowerOpticalPointSize,{os2.UsLowerOpticalPointSize}");
            Console.WriteLine($"os2,UsUpperOpticalPointSize,{os2.UsUpperOpticalPointSize}");
        }

        var hhea = font.HorizontalHeader;
        Console.WriteLine($"hhea,MajorVersion,{hhea.MajorVersion}");
        Console.WriteLine($"hhea,MinorVersion,{hhea.MinorVersion}");
        Console.WriteLine($"hhea,Ascender,{hhea.Ascender}");
        Console.WriteLine($"hhea,Descender,{hhea.Descender}");
        Console.WriteLine($"hhea,LineGap,{hhea.LineGap}");
        Console.WriteLine($"hhea,AdvanceWidthMax,{hhea.AdvanceWidthMax}");
        Console.WriteLine($"hhea,MinLeftSideBearing,{hhea.MinLeftSideBearing}");
        Console.WriteLine($"hhea,MinRightSideBearing,{hhea.MinRightSideBearing}");
        Console.WriteLine($"hhea,XMaxExtent,{hhea.XMaxExtent}");
        Console.WriteLine($"hhea,CaretSlopeRise,{hhea.CaretSlopeRise}");
        Console.WriteLine($"hhea,CaretSlopeRun,{hhea.CaretSlopeRun}");
        Console.WriteLine($"hhea,CaretOffset,{hhea.CaretOffset}");
        Console.WriteLine($"hhea,Reserved1,{hhea.Reserved1}");
        Console.WriteLine($"hhea,Reserved2,{hhea.Reserved2}");
        Console.WriteLine($"hhea,Reserved3,{hhea.Reserved3}");
        Console.WriteLine($"hhea,Reserved4,{hhea.Reserved4}");
        Console.WriteLine($"hhea,MetricDataFormat,{hhea.MetricDataFormat}");
        Console.WriteLine($"hhea,NumberOfHMetrics,{hhea.NumberOfHMetrics}");

        var name = font.Name;
        Console.WriteLine($"name,Format,{name.Format}");
        Console.WriteLine($"name,Count,{name.Count}");
        Console.WriteLine($"name,StringOffset,{name.StringOffset}");
        for (var i = 0; i < name.NameRecords.Length; i++)
        {
            var x = name.NameRecords[i];
            Console.WriteLine($"name,NameRecords[{i}],{x.Name}," +
                $"PlatformID={x.NameRecord.PlatformID}," +
                $"EncodingID={x.NameRecord.EncodingID}," +
                $"LanguageID={x.NameRecord.LanguageID}," +
                $"NameID={x.NameRecord.NameID}," +
                $"Length={x.NameRecord.Length}," +
                $"Offset={x.NameRecord.Offset}");
        }
        Console.WriteLine($"name,LanguageTagCount,{name.LanguageTagCount}");
        for (var i = 0; i < name.LanguageTagRecords.Length; i++)
        {
            var x = name.LanguageTagRecords[i];
            Console.WriteLine($"name,LanguageTagRecords[{i}],{x.Name}," +
                $"Length={x.LanguageTagRecord.Length}," +
                $"LanguageTagOffset={x.LanguageTagRecord.LanguageTagOffset}");
        }

        //var hmtx = font.HorizontalMetrics;
        //var cmap4 = font.CMap;
        //var cmap4_range = font.CMap4;
    }
}
