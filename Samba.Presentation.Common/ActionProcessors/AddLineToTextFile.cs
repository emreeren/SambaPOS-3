using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export]
    class AddLineToTextFile : ActionType
    {
        private readonly ILogService _logService;
        [ImportingConstructor]
        public AddLineToTextFile(ILogService logService)
        {
            _logService = logService;
        }

        public override void Process(ActionData actionData)
        {
            var filePath = actionData.GetAsString("FilePath");
            var text = actionData.GetAsString("Text");
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }
                File.AppendAllText(filePath, text + Environment.NewLine);
            }
            catch (Exception e)
            {
                _logService.LogError(e);
            }
        }

        protected override object GetDefaultData()
        {
            return new { FilePath = "", Text = "" };
        }

        protected override string GetActionName()
        {
            return "Add Line to Text File";
        }

        protected override string GetActionKey()
        {
            return GetType().Name;
        }
    }
}
