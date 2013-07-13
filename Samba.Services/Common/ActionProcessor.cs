using System.Dynamic;
using Samba.Infrastructure;

namespace Samba.Services.Common
{
    public abstract class ActionProcessor : IActionProcessor
    {
        public bool Handles(string actionType)
        {
            return actionType == ActionType;
        }

        private ExpandoObject _parameterObject;
        public ExpandoObject ParameterObject { get { return _parameterObject ?? (_parameterObject = DefaultData.ToDynamic()); } }

        public string ActionType { get { return GetActionKey(); } }
        public string ActionName { get { return GetActionName(); } }
        public object DefaultData { get { return GetDefaultData(); } }

        public abstract void Process(ActionData actionData);
        protected abstract object GetDefaultData();
        protected abstract string GetActionName();
        protected abstract string GetActionKey();
    }
}