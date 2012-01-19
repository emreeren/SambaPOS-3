using System.Collections.Generic;

namespace Samba.Presentation.Common.ModelBase
{
    public interface IValueWithSource
    {
        string PropertyName { get; set; }
        IEnumerable<string> Values { get; }
    }
}
