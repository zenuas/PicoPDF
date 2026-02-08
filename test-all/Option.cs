using Mina.Command;

namespace PicoPDF.TestAll;

public class Option
{
    [CommandOption("debug")]
    public bool Debug { get; init; } = true;

    [CommandOption("unicode")]
    public bool AppendCIDToUnicode { get; init; } = true;

    [CommandOption("embedded-font")]
    public bool EmbeddedFont { get; init; } = false;

    [CommandOption("contents-deflate")]
    public bool ContentsStreamDeflate { get; init; } = false;

    [CommandOption("jpeg-deflate")]
    public bool JpegStreamDeflate { get; init; } = true;

    [CommandOption("image-deflate")]
    public bool ImageStreamDeflate { get; init; } = true;

    [CommandOption("cmap-deflate")]
    public bool CMapStreamDeflate { get; init; } = false;

    [CommandOption("always-update")]
    public bool AlwaysUpdate { get; init; } = true;

    [CommandOption("font-list")]
    public bool FontList { get; init; } = false;

    [CommandOption("all-font-preview")]
    public bool AllFontPreview { get; init; } = false;

    [CommandOption("output-font-file")]
    public string OutputFontFile { get; init; } = "";

    [CommandOption("font-file-extract")]
    public string FontFileExtract { get; init; } = "";

    [CommandOption("font-export-chars")]
    public string FontExportChars { get; init; } = "a";

    [CommandOption("register-system-font")]
    public bool RegisterSystemFont { get; init; } = true;

    [CommandOption("register-user-font")]
    public string RegisterUserFont { get; init; } = "";

    [CommandOption("cmap-list")]
    public bool CMapList { get; init; } = false;

    [CommandOption("input-deflate")]
    public string InputDeflate { get; init; } = "";

    [CommandOption("font-dump")]
    public string FontDump { get; init; } = "";

    [CommandOption("output"), CommandOption('o')]
    public string Output { get; init; } = "";
}
