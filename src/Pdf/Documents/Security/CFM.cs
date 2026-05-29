using System;

namespace PicoPDF.Pdf.Documents.Security;

public enum CFM
{
    None,

    [Obsolete("deprecated")]
    V2,

    AESV2,

    AESV3,
}
