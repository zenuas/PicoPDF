using Mina.Command;
using Mina.Extension;
using OpenType;
using OpenType.Tables.Common;
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

            if (gsub.ScriptList is { } scripts) DumpScriptListRecord("gsub", scripts);
            if (gsub.FeatureList is { } features) DumpFeatureListRecord("gsub", features);
            if (gsub.LookupList is { } lookups) DumpLookupListRecord("gsub", lookups);
        }

        if (font.GlyphPositioning is { } gpos)
        {
            Console.WriteLine($"gpos,MajorVersion,{gpos.MajorVersion}");
            Console.WriteLine($"gpos,MinorVersion,{gpos.MinorVersion}");
            Console.WriteLine($"gpos,ScriptListOffset,{gpos.ScriptListOffset}");
            Console.WriteLine($"gpos,FeatureListOffset,{gpos.FeatureListOffset}");
            Console.WriteLine($"gpos,LookupListOffset,{gpos.LookupListOffset}");
            Console.WriteLine($"gpos,FeatureVariationsOffset,{gpos.FeatureVariationsOffset}");

            if (gpos.ScriptList is { } scripts) DumpScriptListRecord("gpos", scripts);
            if (gpos.FeatureList is { } features) DumpFeatureListRecord("gpos", features);
            if (gpos.LookupList is { } lookups) DumpLookupListRecord("gpos", lookups);
        }
    }

    public static void DumpScriptListRecord(string prefix, ScriptListRecord scripts)
    {
        Console.WriteLine($"{prefix},ScriptCount,{scripts.ScriptCount}");
        for (var i = 0; i < scripts.ScriptRecords.Length; i++)
        {
            var script = scripts.ScriptRecords[i];
            Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTag,{script.ScriptTag}");
            Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptOffset,{script.ScriptOffset}");
            Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.DefaultLangSysOffset,{script.ScriptTable.DefaultLangSysOffset}");
            Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.LangSysCount,{script.ScriptTable.LangSysCount}");
            for (var j = 0; j < script.ScriptTable.LangSysRecords.Length; j++)
            {
                var langsys = script.ScriptTable.LangSysRecords[j];
                Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LangSysTag,{langsys.LangSysTag}");
                Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LangSysOffset,{langsys.LangSysOffset}");
                Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.LookupOrderOffset,{langsys.LanguageSystemTable.LookupOrderOffset}");
                Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.RequiredFeatureIndex,{langsys.LanguageSystemTable.RequiredFeatureIndex}");
                Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.FeatureIndexCount,{langsys.LanguageSystemTable.FeatureIndexCount}");
                for (var k = 0; k < langsys.LanguageSystemTable.FeatureIndices.Length; k++)
                {
                    Console.WriteLine($"{prefix},ScriptRecords[{i}].ScriptTable.LangSysRecords[{j}].LanguageSystemTable.FeatureIndices[{k}],{langsys.LanguageSystemTable.FeatureIndices[k]}");
                }
            }
        }
    }

    public static void DumpFeatureListRecord(string prefix, FeatureListRecord features)
    {
        Console.WriteLine($"{prefix},FeatureCount,{features.FeatureCount}");
        for (var i = 0; i < features.FeatureRecords.Length; i++)
        {
            var feature = features.FeatureRecords[i];
            Console.WriteLine($"{prefix},FeatureRecords[{i}].FeatureTag,{feature.FeatureTag}");
            Console.WriteLine($"{prefix},FeatureRecords[{i}].FeatureOffset,{feature.FeatureOffset}");
            Console.WriteLine($"{prefix},FeatureRecords[{i}].FeatureTable.FeatureParamsOffset,{feature.FeatureTable.FeatureParamsOffset}");
            Console.WriteLine($"{prefix},FeatureRecords[{i}].FeatureTable.LookupIndexCount,{feature.FeatureTable.LookupIndexCount}");
            for (var j = 0; j < feature.FeatureTable.LookupListIndices.Length; j++)
            {
                Console.WriteLine($"{prefix},FeatureRecords[{i}].FeatureTable.LookupListIndices[{j}],{feature.FeatureTable.LookupListIndices[j]}");
            }
        }
    }

    public static void DumpLookupListRecord(string prefix, LookupListRecord lookups)
    {
        Console.WriteLine($"{prefix},LookupList.LookupCount,{lookups.LookupCount}");
        for (var i = 0; i < lookups.LookupRecords.Length; i++)
        {
            var lookup = lookups.LookupRecords[i];
            Console.WriteLine($"{prefix},LookupList.LookupRecords[{i}].LookupOffset,{lookup.LookupOffset}");
            Console.WriteLine($"{prefix},LookupList.LookupRecords[{i}].LookupTable.LookupType,{lookup.LookupTable.LookupType}");
            Console.WriteLine($"{prefix},LookupList.LookupRecords[{i}].LookupTable.LookupFlag,{lookup.LookupTable.LookupFlag}");
            Console.WriteLine($"{prefix},LookupList.LookupRecords[{i}].LookupTable.SubTableCount,{lookup.LookupTable.SubTableCount}");
            for (var j = 0; j < lookup.LookupTable.SubtableOffsets.Length; j++)
            {
                Console.WriteLine($"{prefix},LookupList.LookupRecords[{i}].LookupTable.SubtableOffsets[{j}],{lookup.LookupTable.SubtableOffsets[j]}");
            }
            Console.WriteLine($"{prefix},LookupList.LookupRecords[{i}].LookupTable.MarkFilteringSet,{lookup.LookupTable.MarkFilteringSet}");
            for (var j = 0; j < lookup.LookupTable.Subtables.Length; j++)
            {
                DumpSubtable($"{prefix},LookupList.LookupRecords[{i}].LookupTable.Subtables[{j}]", lookup.LookupTable.Subtables[j]);
            }
        }
    }

    public static void DumpSubtable(string prefix, ISubtable subtable)
    {
        switch (subtable)
        {
            case SingleSubstFormat1 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.CoverageOffset,{x.CoverageOffset}");
                Console.WriteLine($"{prefix}.DeltaGlyphID,{x.DeltaGlyphID}");
                DumpCoverage($"{prefix}.Coverage", x.Coverage);
                break;

            case SingleSubstFormat2 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.CoverageOffset,{x.CoverageOffset}");
                Console.WriteLine($"{prefix}.GlyphCount,{x.GlyphCount}");
                for (var i = 0; i < x.GlyphCount; i++)
                {
                    Console.WriteLine($"{prefix}.SubstituteGlyphIDs[{i}],{x.SubstituteGlyphIDs[i]}");
                }
                DumpCoverage($"{prefix}.Coverage", x.Coverage);
                break;

            case SinglePosFormat1 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.CoverageOffset,{x.CoverageOffset}");
                Console.WriteLine($"{prefix}.ValueFormat,{x.ValueFormat}");
                DumpValueRecord($"{prefix}.ValueRecord", x.ValueRecord);
                DumpCoverage($"{prefix}.Coverage", x.Coverage);
                break;

            case SinglePosFormat2 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.CoverageOffset,{x.CoverageOffset}");
                Console.WriteLine($"{prefix}.ValueFormat,{x.ValueFormat}");
                for (var i = 0; i < x.ValueRecords.Length; i++)
                {
                    DumpValueRecord($"{prefix}.ValueRecord[{i}]", x.ValueRecords[i]);
                }
                DumpCoverage($"{prefix}.Coverage", x.Coverage);
                break;

            case PairPosFormat1 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.CoverageOffset,{x.CoverageOffset}");
                Console.WriteLine($"{prefix}.ValueFormat1,{x.ValueFormat1}");
                Console.WriteLine($"{prefix}.ValueFormat2,{x.ValueFormat2}");
                Console.WriteLine($"{prefix}.PairSetCount,{x.PairSetCount}");
                for (var i = 0; i < x.PairSetOffsets.Length; i++)
                {
                    Console.WriteLine($"{prefix}.PairSetOffsets[{i}],{x.PairSetOffsets[i]}");
                }
                DumpCoverage($"{prefix}.Coverage", x.Coverage);
                for (var i = 0; i < x.PairSets.Length; i++)
                {
                    DumpPairSetTable($"{prefix}.PairSets[{i}]", x.PairSets[i]);
                }
                break;

            case PosExtensionFormat1 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.ExtensionLookupType,{x.ExtensionLookupType}");
                Console.WriteLine($"{prefix}.ExtensionOffset,{x.ExtensionOffset}");
                Console.WriteLine($"{prefix}.Extension,{x.GetType().Name}");
                DumpSubtable(prefix, x.Extension);
                break;

            default:
                Console.WriteLine($"{prefix}.Format,##{subtable.GetType().Name}");
                break;
        }
    }

    public static void DumpCoverage(string prefix, ICoverageFormat coverage)
    {
        switch (coverage)
        {
            case CoverageFormat1 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.GlyphCount,{x.GlyphCount}");
                for (var i = 0; i < x.GlyphArray.Length; i++)
                {
                    Console.WriteLine($"{prefix}.GlyphArray[{i}],{x.GlyphArray[i]}");
                }
                break;

            case CoverageFormat2 x:
                Console.WriteLine($"{prefix}.Format,{x.Format}");
                Console.WriteLine($"{prefix}.RangeCount,{x.RangeCount}");
                for (var i = 0; i < x.RangeRecords.Length; i++)
                {
                    Console.WriteLine($"{prefix}.RangeRecords[{i}].StartGlyphID,{x.RangeRecords[i].StartGlyphID}");
                    Console.WriteLine($"{prefix}.RangeRecords[{i}].EndGlyphID,{x.RangeRecords[i].EndGlyphID}");
                    Console.WriteLine($"{prefix}.RangeRecords[{i}].CoverageIndex,{x.RangeRecords[i].StartCoverageIndex}");
                }
                break;
        }
    }

    public static void DumpValueRecord(string prefix, ValueRecord value)
    {
        Console.WriteLine($"{prefix}.XPlacement,{value.XPlacement}");
        Console.WriteLine($"{prefix}.YPlacement,{value.YPlacement}");
        Console.WriteLine($"{prefix}.XAdvance,{value.XAdvance}");
        Console.WriteLine($"{prefix}.YAdvance,{value.YAdvance}");
        Console.WriteLine($"{prefix}.XPlaDeviceOffset,{value.XPlaDeviceOffset}");
        Console.WriteLine($"{prefix}.YPlaDeviceOffset,{value.YPlaDeviceOffset}");
        Console.WriteLine($"{prefix}.XAdvDeviceOffset,{value.XAdvDeviceOffset}");
        Console.WriteLine($"{prefix}.YAdvDeviceOffset,{value.YAdvDeviceOffset}");

        if (value.XPlaDevice is { }) DumpDeviceTable($"{prefix}.XPlaDevice", value.XPlaDevice);
        if (value.YPlaDevice is { }) DumpDeviceTable($"{prefix}.YPlaDevice", value.YPlaDevice);
        if (value.XAdvDevice is { }) DumpDeviceTable($"{prefix}.XAdvDevice", value.XAdvDevice);
        if (value.YAdvDevice is { }) DumpDeviceTable($"{prefix}.YAdvDevice", value.YAdvDevice);
    }

    public static void DumpDeviceTable(string prefix, DeviceTable value)
    {
        Console.WriteLine($"{prefix}.StartSize,{value.StartSize}");
        Console.WriteLine($"{prefix}.EndSize,{value.EndSize}");
        Console.WriteLine($"{prefix}.DeltaFormat,{value.DeltaFormat}");
        for (var i = 0; i < value.DeltaValue.Length; i++)
        {
            Console.WriteLine($"{prefix}.DeltaValue[{i}],{value.DeltaValue[i]}");
        }
    }

    public static void DumpPairSetTable(string prefix, PairSetTable value)
    {
        Console.WriteLine($"{prefix}.PairValueCount,{value.PairValueCount}");
        for (var i = 0; i < value.PairValueRecords.Length; i++)
        {
            DumpPairValue($"{prefix}.PairValueRecords[{i}]", value.PairValueRecords[i]);
        }
    }

    public static void DumpPairValue(string prefix, PairValue value)
    {
        Console.WriteLine($"{prefix}.SecondGlyph,{value.SecondGlyph}");
        DumpValueRecord($"{prefix}.ValueRecord1", value.ValueRecord1);
        DumpValueRecord($"{prefix}.ValueRecord2", value.ValueRecord2);
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
