using System;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure;
using Samba.Infrastructure.Data.Serializer;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AutomationModule
{
    [Export]
    internal class ActionDataBuilder
    {
        private readonly ICacheService _cacheService;
        private readonly Preprocessor _preprocessor;
        private ActionContainer _actionContainer;
        private AppAction _action;
        private string _containerParameterValues;
        private object _dataObject;

        [ImportingConstructor]
        public ActionDataBuilder(ICacheService cacheService, Preprocessor preprocessor)
        {
            _cacheService = cacheService;
            _preprocessor = preprocessor;
        }

        public ActionDataBuilder CreateFor(ActionContainer actionContainer)
        {
            _actionContainer = actionContainer;
            _action = ObjectCloner.Clone(_cacheService.GetActions().Single(x => x.Id == _actionContainer.AppActionId));
            _containerParameterValues = _actionContainer.ParameterValues ?? "";
            return this;
        }

        public ActionDataBuilder PreprocessWith(object dataObject)
        {
            _dataObject = dataObject;
            _containerParameterValues = _preprocessor.Process(_containerParameterValues, _dataObject);
            _action.Parameter = _preprocessor.Process(_action.Parameter, _dataObject);
            return this;
        }

        public ActionData Build()
        {
            var result = new ActionData
                             {
                                 Action = _action,
                                 DataObject = _dataObject,
                                 ParameterValues = _containerParameterValues
                             };
            return result;
        }

        public void InvokeFor(Action<ActionData> dataAction)
        {
            dataAction.Invoke(Build());
        }
    }
}