using Mina.Command;
using Mina.Extension;
using OpenType;
using OpenType.Tables.PostScript;
using OpenType.Tables.Subtable;
using System;
using System.Globalization;
using System.Linq;

namespace PicoPDF.TestAll;

public class FontDump : FontRegisterCommand
{
    [CommandOption("font")]
    public string Font { get; init; } = "Noto Sans JP Regular";

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadFont(Font);
        Dump(font);
    }

    public static void Dump(IOpenTypeFont font)
    {
        var head = font.FontHeader;
        Console.WriteLine($"head,MajorVersion,{head.MajorVersion}");
        Console.WriteLine($"head,MinorVersion,{head.MinorVersion}");
        Console.WriteLine($"head,FontRevision,{head.FontRevision}");
        Console.WriteLine($"head,ChecksumAdjustment,{head.ChecksumAdjustment}");
        Console.WriteLine($"head,MagicNumber,0x{head.MagicNumber:x8}");
        Console.WriteLine($"head,Flags,{head.Flags}");
        Console.WriteLine($"head,UnitsPerEm,{head.UnitsPerEm}");
        Console.WriteLine($"head,Created,{head.Created.ToDateTime()}");
        Console.WriteLine($"head,Modified,{head.Modified.ToDateTime()}");
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
            Console.WriteLine($"os2,Panose,[{os2.Panose.Select(x => x.ToString(CultureInfo.InvariantCulture)).Join(", ")}]");
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

        if (font is PostScriptFont ps)
        {
            var cff = ps.CompactFontFormat;
            Console.WriteLine($"cff,Major,{cff.Major}");
            Console.WriteLine($"cff,Minor,{cff.Minor}");
            Console.WriteLine($"cff,HeaderSize,{cff.HeaderSize}");
            Console.WriteLine($"cff,OffsetSize,{cff.OffsetSize}");
            for (var i = 0; i < cff.Names.Length; i++)
            {
                Console.WriteLine($"cff,Names[{i}],{cff.Names[i]}");
            }
            DumpTopDict("cff,TopDict", cff.TopDict);
            for (var i = 0; i < cff.Strings.Length; i++)
            {
                Console.WriteLine($"cff,Strings[{i}],{cff.Strings[i]}");
            }
        }

        if (font.GlyphSubstitution is { } gsub)
        {
            Console.WriteLine($"gsub,MajorVersion,{gsub.MajorVersion}");
            Console.WriteLine($"gsub,MinorVersion,{gsub.MinorVersion}");
            Console.WriteLine($"gsub,ScriptListOffset,{gsub.ScriptListOffset}");
            Console.WriteLine($"gsub,FeatureListOffset,{gsub.FeatureListOffset}");
            Console.WriteLine($"gsub,LookupListOffset,{gsub.LookupListOffset}");
            Console.WriteLine($"gsub,FeatureVariationsOffset,{gsub.FeatureVariationsOffset}");

            if (gsub.ScriptList is { } scripts)
            {
                Console.WriteLine($"gsub,ScriptList.Count,{scripts.ScriptCount}");
                for (var i = 0; i < scripts.ScriptRecords.Length; i++)
                {
                    Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTag,{scripts.ScriptRecords[i].ScriptTag}");
                    Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptOffset,{scripts.ScriptRecords[i].ScriptOffset}");
                    Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.DefaultLangSysOffset,{scripts.ScriptRecords[i].ScriptTable.DefaultLangSysOffset}");
                    Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.LangSysCount,{scripts.ScriptRecords[i].ScriptTable.LangSysCount}");
                    for (var j = 0; j < scripts.ScriptRecords[i].ScriptTable.LangSysRecords.Length; j++)
                    {
                        Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LangSysTag,{scripts.ScriptRecords[i].ScriptTable.LangSysRecords[j].LangSysTag}");
                        Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LangSysOffset,{scripts.ScriptRecords[i].ScriptTable.LangSysRecords[j].LangSysOffset}");
                        Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.LookupOrderOffset,{scripts.ScriptRecords[i].ScriptTable.LangSysRecords[j].LanguageSystemTable.LookupOrderOffset}");
                        Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.RequiredFeatureIndex,{scripts.ScriptRecords[i].ScriptTable.LangSysRecords[j].LanguageSystemTable.RequiredFeatureIndex}");
                        Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.FeatureIndexCount,{scripts.ScriptRecords[i].ScriptTable.LangSysRecords[j].LanguageSystemTable.FeatureIndexCount}");
                        for (var k = 0; k < scripts.ScriptRecords[i].ScriptTable.LangSysRecords[j].LanguageSystemTable.FeatureIndices.Length; k++)
                        {
                            Console.WriteLine($"gsub,ScriptList.ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.FeatureIndices[{k}],{scripts.ScriptRecords[i].ScriptTable.LangSysRecords[j].LanguageSystemTable.FeatureIndices[k]}");
                        }
                    }
                }
            }

            if (gsub.FeatureList is { } features)
            {
                Console.WriteLine($"gsub,FeatureList.FeatureCount,{features.FeatureCount}");
                for (var i = 0; i < features.FeatureRecords.Length; i++)
                {
                    Console.WriteLine($"gsub,FeatureList.FeatureRecords[{i}].FeatureTag,{features.FeatureRecords[i].FeatureTag}");
                    Console.WriteLine($"gsub,FeatureList.FeatureRecords[{i}].FeatureOffset,{features.FeatureRecords[i].FeatureOffset}");
                    Console.WriteLine($"gsub,FeatureList.FeatureRecords[{i}].FeatureTable.FeatureParamsOffset,{features.FeatureRecords[i].FeatureTable.FeatureParamsOffset}");
                    Console.WriteLine($"gsub,FeatureList.FeatureRecords[{i}].FeatureTable.LookupIndexCount,{features.FeatureRecords[i].FeatureTable.LookupIndexCount}");
                    for (var j = 0; j < features.FeatureRecords[i].FeatureTable.LookupListIndices.Length; j++)
                    {
                        Console.WriteLine($"gsub,FeatureList.FeatureRecords[{i}].FeatureTable.LookupListIndices[{j}],{features.FeatureRecords[i].FeatureTable.LookupListIndices[j]}");
                    }
                }
            }

            if (gsub.LookupList is { } lookups)
            {
                Console.WriteLine($"gsub,LookupList.LookupCount,{lookups.LookupCount}");
                for (var i = 0; i < lookups.LookupRecords.Length; i++)
                {
                    Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupOffset,{lookups.LookupRecords[i].LookupOffset}");
                    Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.LookupType,{lookups.LookupRecords[i].LookupTable.LookupType}");
                    Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.LookupFlag,{lookups.LookupRecords[i].LookupTable.LookupFlag}");
                    Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.SubTableCount,{lookups.LookupRecords[i].LookupTable.SubTableCount}");
                    Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.SubtableOffsets,{lookups.LookupRecords[i].LookupTable.SubtableOffsets}");
                    Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.MarkFilteringSet,{lookups.LookupRecords[i].LookupTable.MarkFilteringSet}");
                    for (var j = 0; j < lookups.LookupRecords[i].LookupTable.Subtables.Length; j++)
                    {
                        var subtable = lookups.LookupRecords[i].LookupTable.Subtables[j];
                        switch (subtable)
                        {
                            case SingleSubstFormat1 x:
                                Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].Format,{x.Format}");
                                Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].CoverageOffset,{x.CoverageOffset}");
                                Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].DeltaGlyphID,{x.DeltaGlyphID}");
                                DumpCoverage($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].Coverage", x.Coverage);
                                break;

                            case SingleSubstFormat2 x:
                                Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].Format,{x.Format}");
                                Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].CoverageOffset,{x.CoverageOffset}");
                                Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].GlyphCount,{x.GlyphCount}");
                                for (var k = 0; k < x.GlyphCount; k++)
                                {
                                    Console.WriteLine($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].SubstituteGlyphIDs[{k}],{x.SubstituteGlyphIDs[k]}");
                                }
                                DumpCoverage($"gsub,LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}].Coverage", x.Coverage);
                                break;
                        }
                    }
                }
            }
        }
    }

    public static void DumpCoverage(string prefix, ICoverageFormat coverage)
    {
        switch (coverage)
        {
            case CoverageFormat1 x:
                Console.WriteLine($"{prefix},Format,{x.Format}");
                Console.WriteLine($"{prefix},GlyphCount,{x.GlyphCount}");
                for (var i = 0; i < x.GlyphArray.Length; i++)
                {
                    Console.WriteLine($"{prefix},GlyphArray[{i}],{x.GlyphArray[i]}");
                }
                break;

            case CoverageFormat2 x:
                Console.WriteLine($"{prefix},Format,{x.Format}");
                Console.WriteLine($"{prefix},RangeCount,{x.RangeCount}");
                for (var i = 0; i < x.RangeRecords.Length; i++)
                {
                    Console.WriteLine($"{prefix},RangeRecords[{i}].StartGlyphID,{x.RangeRecords[i].StartGlyphID}");
                    Console.WriteLine($"{prefix},RangeRecords[{i}].EndGlyphID,{x.RangeRecords[i].EndGlyphID}");
                    Console.WriteLine($"{prefix},RangeRecords[{i}].CoverageIndex,{x.RangeRecords[i].StartCoverageIndex}");
                }
                break;
        }
    }

    public static void DumpTopDict(string prefix, TopDict top_dict)
    {
        DumpTopDict(prefix, top_dict, TopDictOperators.Version);
        DumpTopDict(prefix, top_dict, TopDictOperators.Notice);
        DumpTopDict(prefix, top_dict, TopDictOperators.Copyright);
        DumpTopDict(prefix, top_dict, TopDictOperators.FullName);
        DumpTopDict(prefix, top_dict, TopDictOperators.FamilyName);
        DumpTopDict(prefix, top_dict, TopDictOperators.Weight);
        DumpTopDict(prefix, top_dict, TopDictOperators.IsFixedPitch);
        DumpTopDict(prefix, top_dict, TopDictOperators.ItalicAngle);
        DumpTopDict(prefix, top_dict, TopDictOperators.UnderlinePosition);
        DumpTopDict(prefix, top_dict, TopDictOperators.UnderlineThickness);
        DumpTopDict(prefix, top_dict, TopDictOperators.PaintType);
        DumpTopDict(prefix, top_dict, TopDictOperators.CharstringType);
        DumpTopDict(prefix, top_dict, TopDictOperators.FontMatrix);
        DumpTopDict(prefix, top_dict, TopDictOperators.UniqueID);
        DumpTopDict(prefix, top_dict, TopDictOperators.FontBBox);
        DumpTopDict(prefix, top_dict, TopDictOperators.StrokeWidth);
        DumpTopDict(prefix, top_dict, TopDictOperators.XUID);
        DumpTopDict(prefix, top_dict, TopDictOperators.Charset);
        DumpTopDict(prefix, top_dict, TopDictOperators.Encoding);
        DumpTopDict(prefix, top_dict, TopDictOperators.CharStrings);
        DumpTopDict(prefix, top_dict, TopDictOperators.Private);
        DumpTopDict(prefix, top_dict, TopDictOperators.SyntheticBase);
        DumpTopDict(prefix, top_dict, TopDictOperators.PostScript);
        DumpTopDict(prefix, top_dict, TopDictOperators.BaseFontName);
        DumpTopDict(prefix, top_dict, TopDictOperators.BaseFontBlend);
        DumpTopDict(prefix, top_dict, TopDictOperators.ROS);
        DumpTopDict(prefix, top_dict, TopDictOperators.CIDFontVersion);
        DumpTopDict(prefix, top_dict, TopDictOperators.CIDFontRevision);
        DumpTopDict(prefix, top_dict, TopDictOperators.CIDFontType);
        DumpTopDict(prefix, top_dict, TopDictOperators.CIDCount);
        DumpTopDict(prefix, top_dict, TopDictOperators.UIDBase);
        DumpTopDict(prefix, top_dict, TopDictOperators.FDArray);
        DumpTopDict(prefix, top_dict, TopDictOperators.FDSelect);
        DumpTopDict(prefix, top_dict, TopDictOperators.FontName);

        if (top_dict.PrivateDict is { } private_dict)
        {
            DumpPrivateDict($"{prefix},PrivateDict", private_dict);
        }

        for (var i = 0; i < top_dict.FontDictArray.Length; i++)
        {
            var fd = top_dict.FontDictArray[i];
            DumpTopDict($"{prefix},FontDictArray[{i}]", fd);
        }
    }

    public static void DumpPrivateDict(string prefix, PrivateDict private_dict)
    {
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.BlueValues);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.OtherBlues);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.FamilyBlues);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.FamilyOtherBlues);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.BlueScale);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.BlueShift);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.BlueFuzz);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.StdHW);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.StdVW);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.StemSnapH);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.StemSnapV);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.ForceBold);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.LanguageGroup);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.ExpansionFactor);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.InitialRandomSeed);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.Subrs);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.DefaultWidthX);
        DumpPrivateDict(prefix, private_dict, PrivateDictOperators.NominalWidthX);
    }

    public static void DumpTopDict(string prefix, TopDict top_dict, TopDictOperators op)
    {
        if (top_dict.Dict.TryGetValue(op, out var x)) Console.WriteLine($"{prefix},{op},{ToString(x)}");
    }

    public static void DumpPrivateDict(string prefix, PrivateDict private_dict, PrivateDictOperators op)
    {
        if (private_dict.Dict.TryGetValue(op, out var x)) Console.WriteLine($"{prefix},{op},{ToString(x)}");
    }

    public static string ToString(IntOrDouble[] array) => array.Select(x => x.ToString()).Join(",");
}
