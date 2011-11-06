using System.Windows.Controls;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Presentation.Common
{
    public static class CommonEventPublisher
    {
        public static void PublishViewAddedEvent(VisibleViewModelBase view)
        {
            view.PublishEvent(EventTopicNames.ViewAdded,true);
        }

        public static void PublishViewClosedEvent(VisibleViewModelBase view)
        {
            view.PublishEvent(EventTopicNames.ViewClosed,true);
        }

        public static void PublishDashboardUnloadedEvent(UserControl userControl)
        {
            userControl.PublishEvent(EventTopicNames.DashboardClosed);
        }
    }
}
