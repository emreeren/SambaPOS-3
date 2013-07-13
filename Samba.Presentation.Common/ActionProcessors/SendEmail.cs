using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class SendEmail : ActionType
    {
        private readonly IEmailService _emailService;

        [ImportingConstructor]
        public SendEmail(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public override void Process(ActionData actionData)
        {
            _emailService.SendEMailAsync(actionData.GetAsString("SMTPServer"),
                 actionData.GetAsString("SMTPUser"),
                 actionData.GetAsString("SMTPPassword"),
                 actionData.GetAsInteger("SMTPPort"),
                 actionData.GetAsString("ToEMailAddress"),
                 actionData.GetAsString("CCEmailAddresses"),
                 actionData.GetAsString("FromEMailAddress"),
                 actionData.GetAsString("Subject"),
                 actionData.GetAsString("EMailMessage"),
                 actionData.GetAsString("FileName"),
                 actionData.GetAsBoolean("DeleteFile"),
                 actionData.GetAsBoolean("BypassSslErrors"));
        }

        protected override object GetDefaultData()
        {
            return
                new
                    {
                        SMTPServer = "",
                        SMTPUser = "",
                        SMTPPassword = "",
                        SMTPPort = 0,
                        ToEMailAddress = "",
                        Subject = "",
                        CCEmailAddresses = "",
                        FromEMailAddress = "",
                        EMailMessage = "",
                        FileName = "",
                        DeleteFile = false,
                        BypassSslErrors = false
                    };
        }

        protected override string GetActionName()
        {
            return Resources.SendEmail;
        }

        protected override string GetActionKey()
        {
            return ActionNames.SendEmail;
        }
    }
}
