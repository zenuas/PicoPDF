using System;

namespace Pdf.Documents.Security;

[Flags]
public enum UserAccessPermissions
{
    AllowPrint = 1 << 2,
    AllowModify = 1 << 3,
    AllowCopy = 1 << 4,
    AllowTextAnnotationsAndFillFormFields = 1 << 5,
    AllowFillExistingInteractiveFormFields = 1 << 8,
    AllowExtractTextAndGraphics = 1 << 9,
    AllowAssembleTheDocument = 1 << 10,
    AllowPrintFaithfulDigitalCopy = 1 << 11,

    Default = unchecked((int)0b_11111111_11111111_11110000_11000000),
}
