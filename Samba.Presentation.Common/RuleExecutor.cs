using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Actions;
using Samba.Services;

namespace Samba.Presentation.Common
{
    public class ActionData
    {
        public AppAction Action { get; set; }
        public string ParameterValues { get; set; }
        public object DataObject { get; set; }

        public T GetDataValue<T>(string parameterName) where T : class
        {
            var property = DataObject.GetType().GetProperty(parameterName);
            if (property != null)
                return property.GetValue(DataObject, null) as T;
            return null;
        }

        public bool GetAsBoolean(string parameterName)
        {
            bool result;
            bool.TryParse(GetAsString(parameterName), out result);
            return result;
        }

        public string GetAsString(string parameterName)
        {
            return Action.GetFormattedParameter(parameterName, DataObject, ParameterValues);
        }

        public decimal GetAsDecimal(string parameterName)
        {
            decimal result;
            decimal.TryParse(GetAsString(parameterName), out result);
            return result;
        }

        public int GetAsInteger(string parameterName)
        {
            int result;
            int.TryParse(GetAsString(parameterName), out result);
            return result;
        }
    }

    public static class RuleExecutor
    {
        private static readonly IApplicationState ApplicationState =
            ServiceLocator.Current.GetInstance(typeof(IApplicationState)) as IApplicationState;

        public static void NotifyEvent(string eventName, object dataObject)
        {
            var rules = AppServices.MainDataContext.Rules.Where(x => x.EventName == eventName);
            foreach (var rule in rules.Where(x => string.IsNullOrEmpty(x.EventConstraints) || SatisfiesConditions(x, dataObject)))
            {
                foreach (var actionContainer in rule.Actions)
                {
                    var container = actionContainer;
                    var action = AppServices.MainDataContext.Actions.SingleOrDefault(x => x.Id == container.AppActionId);
                    if (action != null)
                    {
                        var data = new ActionData { Action = action, DataObject = dataObject, ParameterValues = container.ParameterValues };
                        data.PublishEvent(EventTopicNames.ExecuteEvent, true);
                    }
                }
            }
        }

        private static bool SatisfiesConditions(AppRule appRule, object dataObject)
        {
            var conditions = appRule.EventConstraints.Split('#')
                .Select(x => new RuleConstraintViewModel(x));

            var parameterNames = dataObject.GetType().GetProperties().Select(x => x.Name);

            foreach (var condition in conditions)
            {
                var parameterName = parameterNames.FirstOrDefault(condition.Name.Equals);

                if (!string.IsNullOrEmpty(parameterName))
                {
                    var property = dataObject.GetType().GetProperty(parameterName);
                    var parameterValue = property.GetValue(dataObject, null) ?? "";
                    if (!condition.ValueEquals(parameterValue)) return false;
                }
                else
                {
                    if (condition.Name.StartsWith("SN$"))
                    {
                        var settingName = condition.Name.Replace("SN$", "");
                        var settingValue = condition.Value;
                        if (AppServices.SettingService.GetCustomSetting(settingName).StringValue != settingValue)
                            return false;
                    }
                    if (condition.Name == "TerminalName" && !string.IsNullOrEmpty(condition.Value))
                    {
                        if (!condition.Value.Equals(AppServices.CurrentTerminal.Name))
                        {
                            return false;
                        }
                    }
                    if (condition.Name == "DepartmentName" && !string.IsNullOrEmpty(condition.Value))
                    {
                        if (ApplicationState.CurrentDepartment == null ||
                            !condition.Value.Equals(ApplicationState.CurrentDepartment.Name))
                        {
                            return false;
                        }
                    }

                    if (condition.Name == "UserName" && !string.IsNullOrEmpty(condition.Value))
                    {
                        if (ApplicationState.CurrentLoggedInUser == null ||
                            !condition.Value.Equals(ApplicationState.CurrentLoggedInUser.Name))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

    }
}
