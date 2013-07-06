using System.Collections.Generic;
using Samba.Infrastructure.Helpers;

namespace Samba.Presentation.Services.Common.DataGeneration
{
    internal class ParameterBuilder
    {
        private readonly IDictionary<string, string> _values = new Dictionary<string, string>();
        public ParameterBuilder Add(string key, string value)
        {
            _values.Add(key, value);
            return this;
        }

        public override string ToString()
        {
            return JsonHelper.Serialize(_values);
        }
    }
}