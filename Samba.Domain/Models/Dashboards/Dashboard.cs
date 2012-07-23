using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Dashboards
{
    public class Dashboard : Entity
    {
        public Dashboard()
        {
            _widgets = new List<Widget>();
        }

        private readonly IList<Widget> _widgets;
        public virtual IList<Widget> Widgets
        {
            get { return _widgets; }
        }
    }
}
