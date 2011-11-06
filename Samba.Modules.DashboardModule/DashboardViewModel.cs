using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.DashboardModule
{
    [Export]
    public class DashboardViewModel : ModelListViewModelBase
    {
        public ObservableCollection<DashboardCommandCategory> CategoryView
        {
            get
            {
                var result = new ObservableCollection<DashboardCommandCategory>(
                     PresentationServices.DashboardCommandCategories.OrderBy(x => x.Order));
                return result;
            }
        }

        protected override string GetHeaderInfo()
        {
            return "Dashboard";
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => CategoryView);
        }
    }
}
