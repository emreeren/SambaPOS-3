using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class StartProcess : ActionType
    {
        private readonly ILogService _logService;

        [ImportingConstructor]
        public StartProcess(ILogService logService)
        {
            _logService = logService;
        }

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
                try
                {
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception e)
                {
                    _logService.LogError(e, string.Format("Start Process action [{0}] generated an error. See log file for details.", actionData.Action.Name));
                }
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
