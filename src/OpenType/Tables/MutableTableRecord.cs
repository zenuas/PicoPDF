namespace PicoPDF.OpenType.Tables;

public class MutableTableRecord
{
    public required long Position { get; set; }
    public required uint Checksum { get; set; }
    public required uint Offset { get; set; }
    public required uint Length { get; set; }
}
