using System.Collections.Generic;

namespace Samba.Presentation.Common.ModelBase
{
    public interface IValueWithSource
    {
        string Text { get; set; }
        IEnumerable<string> Values { get; }
    }
}
