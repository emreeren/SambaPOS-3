using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.NavigationModule
{
    [Export]
    public class NavigationViewModel : ObservableObject
    {
        public ObservableCollection<ICategoryCommand> CategoryView
        {
            get
            {
                return new ObservableCollection<ICategoryCommand>(
                    PresentationServices.NavigationCommandCategories.OrderBy(x => x.Order));
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => CategoryView);
        }
    }
}
