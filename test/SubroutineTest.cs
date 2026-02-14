using PicoPDF.OpenType.Tables.PostScript;
using Xunit;

namespace PicoPDF.Test;

public class SubroutineTest
{
    [Fact]
    public void GetSubroutineBias()
    {
        Assert.Equal(Subroutine.GetSubroutineBias(0), 107);
        Assert.Equal(Subroutine.GetSubroutineBias(1239), 107);
        Assert.Equal(Subroutine.GetSubroutineBias(1240), 1131);
        Assert.Equal(Subroutine.GetSubroutineBias(33899), 1131);
        Assert.Equal(Subroutine.GetSubroutineBias(33900), 32768);
    }

    [Fact]
    public void CharstringNumber()
    {
        Assert.Equal(Subroutine.CharstringNumber([]), 0);
        Assert.Equal(Subroutine.CharstringNumber([32]), -107);
        Assert.Equal(Subroutine.CharstringNumber([33]), -106);
        Assert.Equal(Subroutine.CharstringNumber([245]), 106);
        Assert.Equal(Subroutine.CharstringNumber([246]), 107);
        Assert.Equal(Subroutine.CharstringNumber([247, 0]), 108);
        Assert.Equal(Subroutine.CharstringNumber([247, 1]), 109);
        Assert.Equal(Subroutine.CharstringNumber([250, 254]), 1130);
        Assert.Equal(Subroutine.CharstringNumber([250, 255]), 1131);
        Assert.Equal(Subroutine.CharstringNumber([251, 0]), -108);
        Assert.Equal(Subroutine.CharstringNumber([251, 1]), -109);
        Assert.Equal(Subroutine.CharstringNumber([254, 254]), -1130);
        Assert.Equal(Subroutine.CharstringNumber([254, 255]), -1131);
        Assert.Equal(Subroutine.CharstringNumber([28, 0, 0]), 0);
        Assert.Equal(Subroutine.CharstringNumber([28, 0, 1]), 1);
        Assert.Equal(Subroutine.CharstringNumber([28, 127, 254]), 32766);
        Assert.Equal(Subroutine.CharstringNumber([28, 127, 255]), 32767);
        Assert.Equal(Subroutine.CharstringNumber([28, 128, 0]), -32768);
        Assert.Equal(Subroutine.CharstringNumber([28, 128, 1]), -32767);
        Assert.Equal(Subroutine.CharstringNumber([28, 255, 254]), -2);
        Assert.Equal(Subroutine.CharstringNumber([28, 255, 255]), -1);

        // 16-bit signed integer with 16 bits of fraction.
        Assert.Equal(Subroutine.CharstringNumber([255, 0, 0, 0, 0]), 0);
        Assert.Equal(Subroutine.CharstringNumber([255, 127, 255, 255, 255]), 0x7FFFFFFF);
        Assert.Equal(Subroutine.CharstringNumber([255, 128, 0, 0, 0]), -2147483648);
        Assert.Equal(Subroutine.CharstringNumber([255, 255, 255, 255, 255]), -1);
    }
}
