using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Actions
{
    public class AppRule : Entity,IOrderable
    {
        public string EventName { get; set; }
        [StringLength(500)]
        public string EventConstraints { get; set; }

        private IList<ActionContainer> _actions;
        public virtual IList<ActionContainer> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }
        
        public AppRule()
        {
            _actions = new List<ActionContainer>();
        }

        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}
