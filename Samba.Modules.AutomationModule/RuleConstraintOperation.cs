using Samba.Localization.Properties;

namespace Samba.Modules.AutomationModule
{
    public class RuleConstraintOperation
    {
        private readonly string _operation;

        public RuleConstraintOperation(string operation)
        {
            _operation = operation;
        }

        public string Display
        {
            get
            {
                var result = Resources.ResourceManager.GetString(_operation);
                return !string.IsNullOrEmpty(result) ? result : _operation;
            }
        }

        public string Value { get { return _operation; } }
    }
}