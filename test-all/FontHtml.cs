using Mina.Extension;
using System.Collections.Generic;

namespace PicoPDF.TestAll;

public class FontHtml : SvgOutput
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);
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
<head>
    <meta charset="utf-8">
    <title>Font Html | {name}</title>
    <style>
    body
    {@$"{{
        font-family: ""{name}"";
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
        for (var gid = 0u; gid <= 0xFFFF; gid++)
        {
            var found = gid_to_char.TryGetValue(gid, out var c);
            var s = found ? char.ConvertFromUtf32(c) : "";
            Output.WriteLine($"""
    <tr>
        <td>{gid}</td>
        <td>{s}</td>
        <td>{(!found ? "" : "U+" + (c <= 0xFFFF ? c.ToString("x4") : c.ToString("x6")))}</td>
        <td>
""");
            OutputSvg(font, [(c, gid)]);
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
