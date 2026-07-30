using System.Collections.Generic;

namespace Pdf;

public interface IHaveReferences
{
    public IEnumerable<IHaveReferences> GetReferences();
}
