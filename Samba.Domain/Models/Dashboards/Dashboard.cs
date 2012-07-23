using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Dashboards
{
    public class Dashboard : Entity
    {
        private readonly IList<Widget> _widgets;
        public virtual IList<Widget> Widgets
        {
            get { return _widgets; }
        }

        private readonly IList<DashboardMap> _dashboardMaps;
        public virtual IList<DashboardMap> DashboardMaps
        {
            get { return _dashboardMaps; }
        }

        public Dashboard()
        {
            _dashboardMaps = new List<DashboardMap>();
            _widgets = new List<Widget>();
        }

        public DashboardMap AddDasboardMap()
        {
            var result = new DashboardMap();
            DashboardMaps.Add(result);
            return result;
        }
    }
}
