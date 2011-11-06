using System.ComponentModel.DataAnnotations;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class ActionContainer : IOrderable
    {
        public ActionContainer()
        {
            
        }

        public ActionContainer(AppAction ruleAction)
        {
            AppActionId = ruleAction.Id;
            Name = ruleAction.Name;
        }

        public int Id { get; set; }
        public int AppActionId { get; set; }
        public int AppRuleId { get; set; }
        public string Name { get; set; }
        [StringLength(500)]
        public string ParameterValues { get; set; }
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        
    }
}
