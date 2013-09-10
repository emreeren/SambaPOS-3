using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class LoopValues : ActionType
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public LoopValues(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        public override void Process(ActionData actionData)
        {
            var name = actionData.GetAsString("Name");
            var values = actionData.GetAsString("Values");
            if (!string.IsNullOrEmpty(values))
            {
                if (!values.Contains(",") && values.StartsWith("(") && values.EndsWith(")"))
                {
                    var endStr = values.Trim(new[] { '(', ')' });
                    int end;
                    var start = 0;
                    if (endStr.Contains("-"))
                    {
                        var parts = endStr.Split('-');
                        int.TryParse(parts[0], out start);
                        int.TryParse(parts[1], out end);
                    }
                    else int.TryParse(endStr, out end);

                    if (end > 0)
                    {
                        for (var i = start; i < end; i++)
                        {
                            _applicationState.NotifyEvent(RuleEventNames.ValueLooped, GenerateDataObject(actionData, name, i.ToString(CultureInfo.InvariantCulture)));
                        }
                    }
                }
                else if (values.Contains(":") && File.Exists(values))
                {
                    foreach (var value in File.ReadAllLines(values))
                    {
                        _applicationState.NotifyEvent(RuleEventNames.ValueLooped, GenerateDataObject(actionData, name, value));
                    }
                }
                else
                {
                    foreach (var value in values.Split(','))
                    {
                        _applicationState.NotifyEvent(RuleEventNames.ValueLooped, GenerateDataObject(actionData, name, value));
                    }
                }
            }
        }

        private object GenerateDataObject(ActionData actionData, string name, string value)
        {
            actionData.DataObject.Name = name;
            actionData.DataObject.LoopValue = value;
            return actionData.DataObject;
            //return new
            //        {
            //            Ticket = actionData.GetDataValue<Ticket>("Ticket"),
            //            Order = actionData.GetDataValue<Order>("Order"),
            //            Entity = actionData.GetDataValue<Entity>("Entity"),
            //            Name = name,
            //            LoopValue = value
            //        };
        }

        protected override object GetDefaultData()
        {
            return new { Name = "", Values = "" };
        }

        protected override string GetActionName()
        {
            return Resources.LoopValues;
        }

        protected override string GetActionKey()
        {
            return ActionNames.LoopValues;
        }
    }
}
