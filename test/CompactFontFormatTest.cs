using PicoPDF.OpenType.Tables.PostScript;
using System.IO;
using Xunit;

namespace PicoPDF.Test;

public class CompactFontFormatTest
{
    public record IntBytes(int Value, byte[] Bytes);

    public static IntBytes DictDataNumber(int number)
    {
        var bytes = CompactFontFormat.DictDataNumberToBytes(number);
        using var mem = new MemoryStream(bytes[1..]);
        return new(CompactFontFormat.ReadDictDataNumber(bytes[0], mem), bytes);
    }

    [Fact]
    public void DictDataNumberTest()
    {
        Assert.Equal(DictDataNumber(0).Value, 0);

        // size 1
        Assert.Equivalent(DictDataNumber(-107), new IntBytes(-107, [32]));
        Assert.Equivalent(DictDataNumber(-106), new IntBytes(-106, [33]));
        Assert.Equivalent(DictDataNumber(106), new IntBytes(106, [245]));
        Assert.Equivalent(DictDataNumber(107), new IntBytes(107, [246]));
        for (var i = -106; i <= 107; i++)
        {
            Assert.Equal(DictDataNumber(i).Value, i);
        }

        // size 2 positive
        Assert.Equivalent(DictDataNumber(108), new IntBytes(108, [247, 0]));
        Assert.Equivalent(DictDataNumber(109), new IntBytes(109, [247, 1]));
        Assert.Equivalent(DictDataNumber(363), new IntBytes(363, [247, 255]));
        Assert.Equivalent(DictDataNumber(364), new IntBytes(364, [248, 0]));
        Assert.Equivalent(DictDataNumber(1130), new IntBytes(1130, [250, 254]));
        Assert.Equivalent(DictDataNumber(1131), new IntBytes(1131, [250, 255]));
        for (var i = 108; i <= 1131; i++)
        {
            Assert.Equal(DictDataNumber(i).Value, i);
        }

        // size 2 negative
        Assert.Equivalent(DictDataNumber(-108), new IntBytes(-108, [251, 0]));
        Assert.Equivalent(DictDataNumber(-109), new IntBytes(-109, [251, 1]));
        Assert.Equivalent(DictDataNumber(-1130), new IntBytes(-1130, [254, 254]));
        Assert.Equivalent(DictDataNumber(-1131), new IntBytes(-1131, [254, 255]));
        for (var i = -1131; i <= -108; i++)
        {
            Assert.Equal(DictDataNumber(i).Value, i);
        }

        // size 3
        Assert.Equivalent(DictDataNumber(-32768), new IntBytes(-32768, [28, 0x80, 0x00]));
        Assert.Equivalent(DictDataNumber(-32767), new IntBytes(-32767, [28, 0x80, 0x01]));
        Assert.Equivalent(DictDataNumber(32766), new IntBytes(32766, [28, 0x7F, 0xFE]));
        Assert.Equivalent(DictDataNumber(32767), new IntBytes(32767, [28, 0x7F, 0xFF]));

        // size 5
        Assert.Equivalent(DictDataNumber(-2147483648), new IntBytes(-2147483648, [29, 0x80, 0x00, 0x00, 0x00]));
        Assert.Equivalent(DictDataNumber(-2147483647), new IntBytes(-2147483647, [29, 0x80, 0x00, 0x00, 0x01]));
        Assert.Equivalent(DictDataNumber(2147483646), new IntBytes(2147483646, [29, 0x7F, 0xFF, 0xFF, 0xFE]));
        Assert.Equivalent(DictDataNumber(2147483647), new IntBytes(2147483647, [29, 0x7F, 0xFF, 0xFF, 0xFF]));
    }
}
