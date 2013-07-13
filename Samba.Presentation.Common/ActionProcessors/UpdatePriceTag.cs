using System.ComponentModel.Composition;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class UpdatePriceTag : ActionType
    {
        private readonly IDepartmentService _departmentService;
        private readonly IMethodQueue _methodQueue;
        private readonly ITriggerService _triggerService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public UpdatePriceTag(IDepartmentService departmentService, IMethodQueue methodQueue, ITriggerService triggerService,
            IApplicationState applicationState)
        {
            _departmentService = departmentService;
            _methodQueue = methodQueue;
            _triggerService = triggerService;
            _applicationState = applicationState;
        }

        public override void Process(ActionData actionData)
        {
            var priceTag = actionData.GetAsString("PriceTag");
            var departmentName = actionData.GetAsString("DepartmentName");
            _departmentService.UpdatePriceTag(departmentName, priceTag);
            _methodQueue.Queue("ResetCache", () => Helper.ResetCache(_triggerService, _applicationState));
        }

        protected override object GetDefaultData()
        {
            return new { DepartmentName = "", PriceTag = "" };
        }

        protected override string GetActionName()
        {
            return Resources.UpdatePriceTag;
        }

        protected override string GetActionKey()
        {
            return ActionNames.UpdatePriceTag;
        }
    }
}
