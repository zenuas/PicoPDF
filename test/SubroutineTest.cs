using PicoPDF.OpenType.Tables.PostScript;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class SubroutineTest
{
    [Fact]
    public void NumberToBytes()
    {
        Assert.Equal(Subroutine.NumberToBytes(-107), [32]);
        Assert.Equal(Subroutine.NumberToBytes(-106), [33]);
        Assert.Equal(Subroutine.NumberToBytes(106), [245]);
        Assert.Equal(Subroutine.NumberToBytes(107), [246]);
        Assert.Equal(Subroutine.NumberToBytes(108), [247, 0]);
        Assert.Equal(Subroutine.NumberToBytes(109), [247, 1]);
        Assert.Equal(Subroutine.NumberToBytes(1130), [250, 254]);
        Assert.Equal(Subroutine.NumberToBytes(1131), [250, 255]);
        Assert.Equal(Subroutine.NumberToBytes(-108), [251, 0]);
        Assert.Equal(Subroutine.NumberToBytes(-109), [251, 1]);
        Assert.Equal(Subroutine.NumberToBytes(-1130), [254, 254]);
        Assert.Equal(Subroutine.NumberToBytes(-1131), [254, 255]);
        //Assert.Equal(Subroutine.NumberToBytes(0), [28, 0, 0]);
        //Assert.Equal(Subroutine.NumberToBytes(1), [28, 0, 1]);
        Assert.Equal(Subroutine.NumberToBytes(32766), [28, 127, 254]);
        Assert.Equal(Subroutine.NumberToBytes(32767), [28, 127, 255]);
        Assert.Equal(Subroutine.NumberToBytes(-32768), [28, 128, 0]);
        Assert.Equal(Subroutine.NumberToBytes(-32767), [28, 128, 1]);
        //Assert.Equal(Subroutine.NumberToBytes(-2), [28, 255, 254]);
        //Assert.Equal(Subroutine.NumberToBytes(-1), [28, 255, 255]);

        // 16-bit signed integer with 16 bits of fraction.
        //Assert.Equal(Subroutine.NumberToBytes(0), [255, 0, 0, 0, 0]);
        Assert.Equal(Subroutine.NumberToBytes(0x7FFFFFFF), [255, 127, 255, 255, 255]);
        Assert.Equal(Subroutine.NumberToBytes(-2147483648), [255, 128, 0, 0, 0]);
        //Assert.Equal(Subroutine.NumberToBytes(-1), [255, 255, 255, 255, 255]);
    }

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

    [Fact]
    public void NextNumberBytes()
    {
        Assert.Equal(Subroutine.NextNumberBytes(28), 2);
        Assert.Equal(Subroutine.NextNumberBytes(32), 0);
        Assert.Equal(Subroutine.NextNumberBytes(246), 0);
        Assert.Equal(Subroutine.NextNumberBytes(247), 1);
        Assert.Equal(Subroutine.NextNumberBytes(248), 1);
        Assert.Equal(Subroutine.NextNumberBytes(249), 1);
        Assert.Equal(Subroutine.NextNumberBytes(250), 1);
        Assert.Equal(Subroutine.NextNumberBytes(251), 1);
        Assert.Equal(Subroutine.NextNumberBytes(252), 1);
        Assert.Equal(Subroutine.NextNumberBytes(253), 1);
        Assert.Equal(Subroutine.NextNumberBytes(254), 1);
        Assert.Equal(Subroutine.NextNumberBytes(255), 4);
    }

    public static int EnumSubroutinesTest(byte[] charstring)
    {
        var local_subr = new byte[][] {
            /* 0 */ [11],
            /* 1 */ [33, 10, 11], // 1 callsubr
            /* 2 */ [11],
            /* 3 */ [33, 10, 11], // 1 callsubr
            /* 4 */ [34, 29, 11], // 2 callgsubr
        };
        var global_subr = new byte[][] {
            /* 0 */ [11],
            /* 1 */ [33, 29, 11], // 1 callgsubr
            /* 2 */ [35, 29, 11], // 3 callgsubr
            /* 3 */ [11],
        };

        var local_subr_mark = new HashSet<int>();
        var global_subr_mark = new HashSet<int>();

        Subroutine.EnumSubroutines(charstring, local_subr, global_subr, (global, index) => (global ? global_subr_mark : local_subr_mark).Add(index));
        return local_subr_mark.Order().Select(x => 1 << x).Aggregate(0, (acc, x) => acc | x) |
            global_subr_mark.Order().Select(x => 0x100 << x).Aggregate(0, (acc, x) => acc | x);
    }

    [Fact]
    public void EnumSubroutines()
    {
        Assert.Equal(EnumSubroutinesTest([]), 0);
        Assert.Equal(EnumSubroutinesTest([28]), 0);
        Assert.Equal(EnumSubroutinesTest([28, 100]), 0);
        Assert.Equal(EnumSubroutinesTest([28, 100, 200]), 0);

        Assert.Equal(EnumSubroutinesTest([32, 10, 14]), 0b001); // 0 callsubr
        Assert.Equal(EnumSubroutinesTest([33, 10, 14]), 0b010); // 1 callsubr
        Assert.Equal(EnumSubroutinesTest([34, 10, 14]), 0b100); // 2 callsubr
        Assert.Equal(EnumSubroutinesTest([32, 10, 34, 10, 14]), 0b0101); // 0 callsubr 2 callsubr
        Assert.Equal(EnumSubroutinesTest([32, 10, 35, 10, 14]), 0b1011); // 0 callsubr 3 callsubr
        Assert.Equal(EnumSubroutinesTest([36, 10, 14]), 0b1100_00010000); // 4 callsubr

        Assert.Equal(EnumSubroutinesTest([32, 29, 14]), 0b0001_00000000); // 0 callgsubr
        Assert.Equal(EnumSubroutinesTest([33, 29, 14]), 0b0010_00000000); // 1 callgsubr
        Assert.Equal(EnumSubroutinesTest([34, 29, 14]), 0b1100_00000000); // 2 callgsubr
        Assert.Equal(EnumSubroutinesTest([35, 29, 14]), 0b1000_00000000); // 3 callgsubr
    }
}
