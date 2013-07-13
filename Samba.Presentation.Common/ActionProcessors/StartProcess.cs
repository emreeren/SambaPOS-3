using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class StartProcess : ActionType
    {
        public override void Process(ActionData actionData)
        {
            var fileName = actionData.GetAsString("FileName");
            var arguments = actionData.GetAsString("Arguments");
            if (!string.IsNullOrEmpty(fileName))
            {
                var psi = new ProcessStartInfo(fileName, arguments);
                var isHidden = actionData.GetAsBoolean("IsHidden");
                if (isHidden) psi.WindowStyle = ProcessWindowStyle.Hidden;

                var useShellExecute = actionData.GetAsBoolean("UseShellExecute");
                if (useShellExecute) psi.UseShellExecute = true;

                var workingDirectory = actionData.GetAsString("WorkingDirectory");
                if (!string.IsNullOrEmpty(workingDirectory))
                    psi.WorkingDirectory = workingDirectory;

                System.Diagnostics.Process.Start(psi);
            }
        }

        protected override object GetDefaultData()
        {
            return new { FileName = "", Arguments = "", UseShellExecute = false, IsHidden = false };
        }

        protected override string GetActionName()
        {
            return Resources.StartProcess;
        }

        protected override string GetActionKey()
        {
            return ActionNames.StartProcess;
        }
    }
}
