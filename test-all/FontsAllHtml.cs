using Mina.Command;
using Mina.Extension;
using PicoPDF.Pdf.Font;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public class FontsAllHtml : SvgOutput
{
    [CommandOption("char"), CommandOption('c')]
    public void SetChar(string s)
    {
        if (s.Length == 0) return;
        Char = s.SubstringAsCount(0, 2) switch
        {
            "0x" => Convert.ToInt32(s[2..], 16),
            "0o" => Convert.ToInt32(s[2..], 8),
            "0b" => Convert.ToInt32(s[2..], 2),
            "U+" => Convert.ToInt32(s[2..], 16),
            _ => int.TryParse(s, out var n) ? n : char.ConvertToUtf32(s, 0),
        };
    }

    public int Char { get; set; } = 'A';

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister().Cast<FontRegister>();

        Output.WriteLine($"""
<!doctype html>
<head>
    <meta charset="utf-8">
    <title>Fonts All Html</title>
</head>

<body>
<table border="1">
<thead>
    <tr>
        <th>Font</th>
        <th>GID</th>
        <th>Char</th>
        <th>Code</th>
        <th>Outline</th>
    </tr>
</thead>
<tbody>
""");
        foreach (var kv in fontreg.Fonts.Where(x => x.Key == FontRegister.GetFontFilePath(x.Value.Value.Path)))
        {
            var font = fontreg.LoadComplete(kv.Key);
            var gid = font.CharToGID(Char);
            if (gid == 0) continue;

            var s = char.ConvertFromUtf32(Char);
            var name = font.Name.NameRecords.FindFirstOrNullValue(x => x.NameRecord.NameID == 1)?.Name ?? font.PostScriptName;
            Output.WriteLine($"""
    <tr>
        <td>{name}</td>
        <td>{gid}</td>
        <td style="font-family: '{name}';">{s}</td>
        <td>U+{(Char <= 0xFFFF ? Char.ToString("x4") : Char.ToString("x6"))}</td>
        <td>
""");
            OutputSvg(font, [Char]);
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
