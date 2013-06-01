using System.Dynamic;

namespace Samba.Services.Common
{
    public class RuleEvent
    {
        public string EventKey { get; set; }
        public string EventName { get; set; }
        public ExpandoObject ParameterObject { get; set; }
    }
}