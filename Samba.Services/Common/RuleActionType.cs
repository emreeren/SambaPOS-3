using System.Dynamic;

namespace Samba.Services.Common
{
    public class RuleActionType
    {
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public ExpandoObject ParameterObject { get; set; }
    }
}