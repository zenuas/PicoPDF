using System;

namespace Binder;

public class ClearableDynamicValue
{
    public required dynamic? Value { get; set; }
    public required Action<ClearableDynamicValue> Clear { get; init; }

    public override string ToString() => $"{Value}";
}
