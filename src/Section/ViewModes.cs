namespace PicoPDF.Section;

public enum ViewModes
{
    None = 0x0000,

    MODES = 0x00FF,
    Every = 0x0001,
    First = 0x0002,
    Last = 0x0003,

    POSITION = 0xFF00,
    Detail = 0x0100 | Every,
    Header = 0x0200,
    Footer = 0x0300,
    Total = 0x0400,
}
