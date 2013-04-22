using System;
using System.Collections.Generic;

namespace Samba.Services.Common
{
    public interface IParameterValue
    {
        string Name { get; }
        string NameDisplay { get; }
        Type ValueType { get; }
        string Value { get; set; }
        IEnumerable<string> Values { get; }
    }
}