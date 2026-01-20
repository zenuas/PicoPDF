using Mina.Extension;
using PicoPDF.OpenType.Tables.PostScript;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace PicoPDF.Test;

public class CompactFontFormatTest
{
    public record IntBytes(int Value, byte[] Bytes);

    public static IntBytes DictDataNumber(int number)
    {
        var bytes = DictData.DictDataNumberToBytes(number);
        return new(DictData.ReadDictDataNumber(bytes[0], bytes[1..].AsSpan()), bytes);
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

    [Fact]
    public void DoubleToPackedBCDTest()
    {
        Assert.Equal(DictData.DoubleToPackedBCD(0), [0]);
        Assert.Equal(DictData.DoubleToPackedBCD(1.40541e-5), [0x01, 0x0a, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0c, 0x00, 0x05]);
        Assert.Equal(DictData.DoubleToPackedBCD(1.40541e17), [0x01, 0x0a, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0b, 0x01, 0x07]);
    }

    [Fact]
    public void PackedBCDToDoubleTest()
    {
        Assert.Equal(DictData.PackedBCDToDouble([0x00]), 0);
        Assert.Equal(DictData.PackedBCDToDouble([0x00, 0x0a, 0x01, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0c, 0x03]), 0.140541e-3);
        Assert.Equal(DictData.PackedBCDToDouble([0x00, 0x0a, 0x01, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0b, 0x03]), 0.140541e3);
    }

    public static IEnumerable<byte> MakeBytes(int count)
    {
        byte b = 0;
        for (var i = 0; i < count; i++)
        {
            yield return b++;
        }
    }

    public static void Write3Bytes(Stream stream, int x) => stream.Write([(byte)((x >> 16) & 0xFF), (byte)((x >> 8) & 0xFF), (byte)(x & 0xFF)]);

    public static int Read3Bytes(Stream stream) => (stream.ReadUByte() << 16) | (stream.ReadUByte() << 8) | stream.ReadUByte();

    public byte[] bytes254_ = [.. MakeBytes(254)];
    public byte[] bytes255_ = [.. MakeBytes(255)];
    public byte[] bytes65279_ = [.. MakeBytes(65279)];
    public byte[] bytes65280_ = [.. MakeBytes(65280)];
    //public byte[] bytes16711679_ = [.. MakeBytes(16711679)];
    //public byte[] bytes16711680_ = [.. MakeBytes(16711680)];

    [Fact]
    public void ReadIndexData0Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(0);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 0);
    }

    [Fact]
    public void ReadIndexData1_0Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(1); // count
        stream.WriteByte(1); // offset size
        stream.WriteByte(1); // offset[0]
        stream.WriteByte(1); // offset[1]
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 1);
        Assert.Equal(result[0].Length, 0);
    }

    [Fact]
    public void ReadIndexData2_0_254Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(2); // count
        stream.WriteByte(1); // offset size
        stream.WriteByte(1); // offset[0]
        stream.WriteByte(1); // offset[1]
        stream.WriteByte(255); // offset[2]
        stream.Write(bytes254_);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 2);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 254);
        Assert.Equal(result[1], bytes254_);
    }

    [Fact]
    public void ReadIndexData2_0_255Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(2); // count
        stream.WriteByte(2); // offset size
        stream.WriteUShortByBigEndian(1); // offset[0]
        stream.WriteUShortByBigEndian(1); // offset[1]
        stream.WriteUShortByBigEndian(256); // offset[2]
        stream.Write(bytes255_);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 2);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], bytes255_);
    }

    [Fact]
    public void ReadIndexData3_0_255_65279Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(3); // count
        stream.WriteByte(2); // offset size
        stream.WriteUShortByBigEndian(1); // offset[0]
        stream.WriteUShortByBigEndian(1); // offset[1]
        stream.WriteUShortByBigEndian(256); // offset[2]
        stream.WriteUShortByBigEndian(65535); // offset[3]
        stream.Write(bytes255_);
        stream.Write(bytes65279_);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 3);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], bytes255_);
        Assert.Equal(result[2].Length, 65279);
        Assert.Equal(result[2], bytes65279_);
    }

    [Fact]
    public void ReadIndexData3_0_255_65280Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(3); // count
        stream.WriteByte(3); // offset size
        Write3Bytes(stream, 1); // offset[0]
        Write3Bytes(stream, 1); // offset[1]
        Write3Bytes(stream, 256); // offset[2]
        Write3Bytes(stream, 65536); // offset[3]
        stream.Write(bytes255_);
        stream.Write(bytes65280_);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 3);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], bytes255_);
        Assert.Equal(result[2].Length, 65280);
        Assert.Equal(result[2], bytes65280_);
    }

    [Fact]
    public void ReadIndexData4_0_255_65280_16711679Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(4); // count
        stream.WriteByte(3); // offset size
        Write3Bytes(stream, 1); // offset[0]
        Write3Bytes(stream, 1); // offset[1]
        Write3Bytes(stream, 256); // offset[2]
        Write3Bytes(stream, 65536); // offset[3]
        Write3Bytes(stream, 16777215); // offset[4]
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexDataHeader(stream);
        Assert.Equal(result.Length, 5);
        Assert.Equal(result[0], 1);
        Assert.Equal(result[1], 1);
        Assert.Equal(result[2], 256);
        Assert.Equal(result[3], 65536);
        Assert.Equal(result[4], 16777215);
    }

    [Fact]
    public void ReadIndexData4_0_255_65280_16711680Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(4); // count
        stream.WriteByte(4); // offset size
        stream.WriteUIntByBigEndian(1); // offset[0]
        stream.WriteUIntByBigEndian(1); // offset[1]
        stream.WriteUIntByBigEndian(256); // offset[2]
        stream.WriteUIntByBigEndian(65536); // offset[3]
        stream.WriteUIntByBigEndian(16777216); // offset[4]
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexDataHeader(stream);
        Assert.Equal(result.Length, 5);
        Assert.Equal(result[0], 1);
        Assert.Equal(result[1], 1);
        Assert.Equal(result[2], 256);
        Assert.Equal(result[3], 65536);
        Assert.Equal(result[4], 16777216);
    }

    [Fact]
    public void WriteIndexData0Test()
    {
        byte[][] bytes = [];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
        Assert.Equal(stream.Position, 2);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 0);

        Assert.Equal(stream.Position, 2);
    }

    [Fact]
    public void WriteIndexData1_0Test()
    {
        byte[][] bytes = [[]];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
        Assert.Equal(stream.Position, 5);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 1); // count
        Assert.Equal(stream.ReadByte(), 1); // offset size
        Assert.Equal(stream.ReadByte(), 1); // offset[0]
        Assert.Equal(stream.ReadByte(), 1); // offset[1]

        Assert.Equal(stream.Position, 5);
    }

    [Fact]
    public void WriteIndexData2_0_254Test()
    {
        byte[][] bytes = [[], bytes254_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
        Assert.Equal(stream.Position, 260);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 2); // count
        Assert.Equal(stream.ReadByte(), 1); // offset size
        Assert.Equal(stream.ReadByte(), 1); // offset[0]
        Assert.Equal(stream.ReadByte(), 1); // offset[1]
        Assert.Equal(stream.ReadByte(), 255); // offset[2]
        Assert.Equal(stream.ReadExactly(254), bytes254_);

        Assert.Equal(stream.Position, 260);
    }

    [Fact]
    public void WriteIndexData2_0_255Test()
    {
        byte[][] bytes = [[], bytes255_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
        Assert.Equal(stream.Position, 264);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 2); // count
        Assert.Equal(stream.ReadByte(), 2); // offset size
        Assert.Equal(stream.ReadUShortByBigEndian(), 1); // offset[0]
        Assert.Equal(stream.ReadUShortByBigEndian(), 1); // offset[1]
        Assert.Equal(stream.ReadUShortByBigEndian(), 256); // offset[2]
        Assert.Equal(stream.ReadExactly(255), bytes255_);

        Assert.Equal(stream.Position, 264);
    }

    [Fact]
    public void WriteIndexData3_0_255_65279Test()
    {
        byte[][] bytes = [[], bytes255_, bytes65279_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
        Assert.Equal(stream.Position, 65545);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 3); // count
        Assert.Equal(stream.ReadByte(), 2); // offset size
        Assert.Equal(stream.ReadUShortByBigEndian(), 1); // offset[0]
        Assert.Equal(stream.ReadUShortByBigEndian(), 1); // offset[1]
        Assert.Equal(stream.ReadUShortByBigEndian(), 256); // offset[2]
        Assert.Equal(stream.ReadUShortByBigEndian(), 65535); // offset[3]
        Assert.Equal(stream.ReadExactly(255), bytes255_);
        Assert.Equal(stream.ReadExactly(65279), bytes65279_);

        Assert.Equal(stream.Position, 65545);
    }

    [Fact]
    public void WriteIndexData3_0_255_65280Test()
    {
        byte[][] bytes = [[], bytes255_, bytes65280_];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexData(stream, bytes);
        Assert.Equal(stream.Position, 65550);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 3); // count
        Assert.Equal(stream.ReadByte(), 3); // offset size
        Assert.Equal(Read3Bytes(stream), 1); // offset[0]
        Assert.Equal(Read3Bytes(stream), 1); // offset[1]
        Assert.Equal(Read3Bytes(stream), 256); // offset[2]
        Assert.Equal(Read3Bytes(stream), 65536); // offset[3]
        Assert.Equal(stream.ReadExactly(255), bytes255_);
        Assert.Equal(stream.ReadExactly(65280), bytes65280_);

        Assert.Equal(stream.Position, 65550);
    }

    [Fact]
    public void WriteIndexData4_0_255_65280_16711679Test()
    {
        uint[] bytes = [0, 255, 65280, 16711679];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexDataHeader(stream, bytes);
        Assert.Equal(stream.Position, 18);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 4); // count
        Assert.Equal(stream.ReadByte(), 3); // offset size
        Assert.Equal(Read3Bytes(stream), 1); // offset[0]
        Assert.Equal(Read3Bytes(stream), 1); // offset[1]
        Assert.Equal(Read3Bytes(stream), 256); // offset[2]
        Assert.Equal(Read3Bytes(stream), 65536); // offset[3]
        Assert.Equal(Read3Bytes(stream), 16777215); // offset[4]

        Assert.Equal(stream.Position, 18);
    }

    [Fact]
    public void WriteIndexData4_0_255_65280_16711680Test()
    {
        uint[] bytes = [0, 255, 65280, 16711680];
        var stream = new MemoryStream();

        CompactFontFormat.WriteIndexDataHeader(stream, bytes);
        Assert.Equal(stream.Position, 23);
        stream.Position = 0;

        Assert.Equal(stream.ReadUShortByBigEndian(), 4); // count
        Assert.Equal(stream.ReadByte(), 4); // offset size
        Assert.Equal<uint>(stream.ReadUIntByBigEndian(), 1); // offset[0]
        Assert.Equal<uint>(stream.ReadUIntByBigEndian(), 1); // offset[1]
        Assert.Equal<uint>(stream.ReadUIntByBigEndian(), 256); // offset[2]
        Assert.Equal<uint>(stream.ReadUIntByBigEndian(), 65536); // offset[3]
        Assert.Equal<uint>(stream.ReadUIntByBigEndian(), 16777216); // offset[4]

        Assert.Equal(stream.Position, 23);
    }
}
