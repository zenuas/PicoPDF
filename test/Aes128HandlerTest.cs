using PicoPDF.Pdf.Documents.Security;
using Xunit;

namespace PicoPDF.Test;

public class Aes128HandlerTest
{
    [Fact]
    public void ComputeEncryptionKey_Revision4()
    {
        using var aes = Aes128Handler.InitializeWithoutGenerateIV(
            [0x08, 0xe2, 0x08, 0x4d, 0xb2, 0x49, 0xdc, 0x76, 0x13, 0x0f, 0xad, 0x44, 0x13, 0x08, 0xc8, 0xd9], 5, 0);
        Assert.Equal(aes.Key, [0x3f, 0x52, 0x67, 0x3b, 0x7b, 0xe3, 0xbb, 0x49, 0x54, 0xb1, 0x7f, 0x38, 0x1a, 0xfa, 0x86, 0x99]);
    }

    [Fact]
    public void DecryptTest()
    {
        var handler = new Aes128Handler { Key = [0x08, 0xe2, 0x08, 0x4d, 0xb2, 0x49, 0xdc, 0x76, 0x13, 0x0f, 0xad, 0x44, 0x13, 0x08, 0xc8, 0xd9] };
        using var encrypter1 = handler.CreateEncrypter(5, 0);
        using var decrypter1 = handler.CreateDecrypter(5, 0);

        var null_text = new byte[] { };
        var cipher_text1 = encrypter1.Filter(null_text);
        Assert.Equal(cipher_text1.Length, 16);
        var plain_text1 = decrypter1.Filter(cipher_text1);
        Assert.Equal(plain_text1, null_text);

        using var encrypter2 = handler.CreateEncrypter(5, 0);
        using var decrypter2 = handler.CreateDecrypter(5, 0);

        var short_text = new byte[] { 0x48, 0x65, 0x6c, 0x24, 0x97 };
        var cipher_text2 = encrypter2.Filter(short_text);
        var plain_text2 = decrypter2.Filter(cipher_text2);
        Assert.Equal(plain_text2, short_text);
    }
}
