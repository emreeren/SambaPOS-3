using System;
using System.ComponentModel.Composition;
using System.IO;
using Samba.Localization.Properties;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
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
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
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
            return Resources.AddLineToTextFile;
        }

        protected override string GetActionKey()
        {
            return GetType().Name;
        }
    }
}
