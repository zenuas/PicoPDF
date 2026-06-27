using Pdf.Documents.Security;
using System;

namespace Pdf.Documents;

public partial class Document
{
    public TrailerInfo? Info { get; init; } = null;
    public IStandardEncryption? Encrypt { get; init; } = null;
    public (byte[] CreateID, byte[] UpdateID)? DocumentID { get; init; }

    public static byte[] GenerateID() => Guid.NewGuid().ToByteArray();
}
