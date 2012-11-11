using System.Collections.Generic;

namespace Samba.Presentation.Services.Common
{
    public interface IRuleConstraint
    {
        string Name { get; set; }
        string NameDisplay { get; }
        string Value { get; set; }
        IEnumerable<string> Values { get; }
        string Operation { get; set; }
        string[] Operations { get; set; }
        string GetConstraintData();
        bool ValueEquals(object parameterValue);
    }
}
