using System.ComponentModel.Composition;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services.Common;

namespace Samba.Presentation.Controls.ActionProcessors
{
    [Export(typeof(IActionType))]
    class DisplayPopup : ActionType
    {
        private readonly IUserInteraction _userInteraction;

        [ImportingConstructor]
        public DisplayPopup(IUserInteraction userInteraction)
        {
            _userInteraction = userInteraction;
        }

        public override void Process(ActionData actionData)
        {
            if (actionData.Action.ActionType == "DisplayPopup")
            {
                var name = actionData.GetAsString("Name");
                var title = actionData.GetAsString("Title");
                var message = actionData.GetAsString("Message");
                var color = actionData.GetAsString("Color");
                color = string.IsNullOrEmpty(color.Trim()) ? "DarkRed" : color;
                if (!string.IsNullOrEmpty(message.Trim()))
                    _userInteraction.DisplayPopup(name, title, message, color);
            }
        }

        protected override object GetDefaultData()
        {
            return new { Name = "", Title = "", Message = "", Color = "" };
        }

        protected override string GetActionName()
        {
            return Resources.DisplayPopup;
        }

        protected override string GetActionKey()
        {
            return "DisplayPopup";
        }
    }
}
