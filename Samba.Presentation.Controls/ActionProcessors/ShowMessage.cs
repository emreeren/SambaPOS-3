using System.ComponentModel.Composition;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services.Common;

namespace Samba.Presentation.Controls.ActionProcessors
{
    [Export(typeof(IActionProcessor))]
    class ShowMessage : ActionProcessor
    {
        private readonly IUserInteraction _userInteraction;

        [ImportingConstructor]
        public ShowMessage(IUserInteraction userInteraction)
        {
            _userInteraction = userInteraction;
        }

        public override void Process(ActionData actionData)
        {
            var param = actionData.GetAsString("Message");
            if (!string.IsNullOrEmpty(param))
                _userInteraction.GiveFeedback(param);
        }

        protected override object GetDefaultData()
        {
            return new { Message = "" };
        }

        protected override string GetActionName()
        {
            return Resources.ShowMessage;
        }

        protected override string GetActionKey()
        {
            return "ShowMessage";
        }
    }
}
