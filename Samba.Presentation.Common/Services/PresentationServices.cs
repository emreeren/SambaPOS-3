using System.Collections.ObjectModel;
using System.Linq;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common.Services
{
    public static class PresentationServices
    {
        public static ObservableCollection<ICategoryCommand> NavigationCommandCategories { get; private set; }
        public static ObservableCollection<DashboardCommandCategory> DashboardCommandCategories { get; private set; }

        public static void Initialize()
        {
            NavigationCommandCategories = new ObservableCollection<ICategoryCommand>();
            DashboardCommandCategories = new ObservableCollection<DashboardCommandCategory>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<ICategoryCommand>>().Subscribe(OnCommandAdded);
        }

        private static void OnCommandAdded(EventParameters<ICategoryCommand> result)
        {
            if (result.Topic == EventTopicNames.NavigationCommandAdded)
                NavigationCommandCategories.Add(result.Value);

            if (result.Topic == EventTopicNames.DashboardCommandAdded)
            {
                var cat = DashboardCommandCategories.FirstOrDefault(item => item.Category == result.Value.Category);
                if (cat == null)
                {
                    cat = new DashboardCommandCategory(result.Value.Category);
                    DashboardCommandCategories.Add(cat);
                }
                if (result.Value.Order > cat.Order)
                    cat.Order = result.Value.Order;
                cat.AddCommand(result.Value);
            }
        }
    }
}
