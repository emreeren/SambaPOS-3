using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for TicketView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketView : UserControl
    {
        internal GridLength ButtonColumnLenght = new GridLength(1, GridUnitType.Star);
        internal GridLength TicketColumnLenght = new GridLength(4, GridUnitType.Star);
        internal Thickness LandscapeCommandWidth = new Thickness(0);
        internal Thickness PortraitCommandWidth = new Thickness(5, 0, 0, 0);

        [ImportingConstructor]
        public TicketView(TicketViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnEvent);
        }

        private void OnEvent(EventParameters<EventAggregator> obj)
        {
            switch (obj.Topic)
            {
                case EventTopicNames.DisableLandscape:
                    DisableLandscapeMode();
                    break;
                case EventTopicNames.EnableLandscape:
                    EnableLandscapeMode();
                    break;
            }
        }

        private void EnableLandscapeMode()
        {
            SwapColumns();
            Column1.Width = ButtonColumnLenght;
            Column2.Width = TicketColumnLenght;
            CommandButtonsColumn.Margin = LandscapeCommandWidth;
        }

        private void DisableLandscapeMode()
        {
            SwapColumns();
            Column1.Width = TicketColumnLenght;
            Column2.Width = ButtonColumnLenght;
            CommandButtonsColumn.Margin = PortraitCommandWidth;
        }

        private void SwapColumns()
        {
            var c1Items = MainGrid.Children.Cast<UIElement>().Where(x => Grid.GetColumn(x) == 0).ToList();
            var c2Items = MainGrid.Children.Cast<UIElement>().Where(x => Grid.GetColumn(x) == 1).ToList();
            c1Items.ForEach(x => Grid.SetColumn(x, 1));
            c2Items.ForEach(x => Grid.SetColumn(x, 0));

        }
    }
}
