namespace Image.Png;

public enum ChunkTypes : uint
{
    IHDR = 0x49484452,
    PLTE = 0x504C5445,
    IDAT = 0x49444154,
    IEND = 0x49454E44,

    cHRM = 0x6348524D,
    gAMA = 0x67414D41,
    iCCP = 0x69434350,
    sBIT = 0x73424954,
    sRGB = 0x73524742,
    bKGD = 0x624B4744,
    hIST = 0x68495354,
    tRNS = 0x74524E53,
    pHYs = 0x70485973,
    sPLT = 0x73504C54,
    tIME = 0x74494D45,
    iTXt = 0x69545874,
    tEXt = 0x74455874,
    zTXt = 0x7A545874,
}
