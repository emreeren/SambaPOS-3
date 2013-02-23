using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Automation
{
    public class ActionContainer : ValueClass, IOrderable
    {
        public ActionContainer()
        {

        }

        public ActionContainer(AppAction ruleAction)
        {
            AppActionId = ruleAction.Id;
            Name = ruleAction.Name;
            ParameterValues = "";
        }

        public int AppActionId { get; set; }
        public int AppRuleId { get; set; }
        public string Name { get; set; }
        public string ParameterValues { get; set; }
        public string CustomConstraint { get; set; }
        public int SortOrder { get; set; }

        public string UserString
        {
            get { return Name; }
        }

    }
}
