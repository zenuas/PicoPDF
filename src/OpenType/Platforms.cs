﻿using System;

namespace PicoPDF.OpenType;

public enum Platforms
{
    Unicode = 0,

    [Obsolete("deprecated")]
    Macintosh = 1,

    [Obsolete("deprecated")]
    ISO = 2,

    Windows = 3,

    Custom = 4,
}
