namespace Samba.Services.Common
{
    public abstract class ActionProcessor : IActionProcessor
    {
        private object _defaultData;

        public bool Handles(string actionType)
        {
            return actionType == ActionKey;
        }

        public string ActionKey { get { return GetActionKey(); } }
        public string ActionName { get { return GetActionName(); } }
        public object DefaultData { get { return _defaultData ?? (_defaultData = GetDefaultData()); } }

        public abstract void Process(ActionData actionData);
        protected abstract object GetDefaultData();
        protected abstract string GetActionName();
        protected abstract string GetActionKey();
    }
}