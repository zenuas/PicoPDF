using Mina.Command;
using Mina.Extension;
using OpenType;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.TestAll;

public class FontHtml : SvgOutput
{
    [CommandOption("font-char-em"), CommandOption('e')]
    public float FontCharEm { get; init; } = 3.0F;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);

        OutputHtml(font,
            args.Length == 0 ?
                Lists.RangeTo(0, font.MaximumProfile.NumberOfGlyphs).Select(x => (uint)x) :
                [.. args.Select(x => x.ToUtf32CharArray()).Flatten().Select(x => font.CharToGID(x))]);
    }


    public void OutputHtml(IOpenTypeFont font, IEnumerable<uint> gids)
    {
        var name = font.Name.NameRecords.FindFirstOrNullValue(x => x.NameRecord.NameID == 1)?.Name ?? font.PostScriptName;

        var gid_to_char = new Dictionary<uint, int>();
        for (var i = 0; i <= 0xFF_FFFF; i++)
        {
            var gid = font.CharToGID(i);
            if (gid == 0) continue;
            gid_to_char[gid] = i;
        }

        Output.WriteLine($"""
<!doctype html>
<html>
<head>
    <meta charset="utf-8">
    <title>Font Html | {name}</title>
    <style>
    body
    {@$"{{
        font-family: ""{name}"";
    }}"}
    .char
    {@$"{{
        font-size: {FontCharEm}em;
    }}"}
    </style>
</head>

<body>
<table border="1">
<thead>
    <tr>
        <th>GID</th>
        <th>Char</th>
        <th>Code</th>
        <th>Outline</th>
    </tr>
</thead>
<tbody>
""");
        foreach (var gid in gids)
        {
            var found = gid_to_char.TryGetValue(gid, out var c);
            var s = found ? char.ConvertFromUtf32(c) : "";
            Output.WriteLine($"""
    <tr>
        <td>{gid}</td>
        <td class="char">{s}</td>
        <td>{(!found ? "" : "U+" + (c <= 0xFFFF ? c.ToString("x4") : c.ToString("x6")))}</td>
        <td>
""");
            OutputSvg(font, [[(c, gid)]], $"g{gid}");
            Output.WriteLine("""
        </td>
    </tr>
""");
        }
        Output.WriteLine("""
</tbody>
</table>
</body>
</html>
""");
        Output.Flush();
    }
}
