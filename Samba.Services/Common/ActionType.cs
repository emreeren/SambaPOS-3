using System.Dynamic;
using Samba.Infrastructure;

namespace Samba.Services.Common
{
    public abstract class ActionType : IActionType
    {
        public bool Handles(string actionType)
        {
            return actionType == ActionKey;
        }

        private ExpandoObject _parameterObject;
        public ExpandoObject ParameterObject { get { return _parameterObject ?? (_parameterObject = DefaultData.ToDynamic()); } }

        public string ActionKey { get { return GetActionKey(); } }
        public string ActionName { get { return GetActionName(); } }
        public object DefaultData { get { return GetDefaultData(); } }

        public abstract void Process(ActionData actionData);
        protected abstract object GetDefaultData();
        protected abstract string GetActionName();
        protected abstract string GetActionKey();
    }
}