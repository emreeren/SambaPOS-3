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
        }

        private void DisableLandscapeMode()
        {
            SwapColumns();
        }

        private void SwapColumns()
        {
            var cd = MainGrid.ColumnDefinitions[0];
            MainGrid.ColumnDefinitions.RemoveAt(0);
            MainGrid.ColumnDefinitions.Add(cd);
            var c1Items = MainGrid.Children.Cast<UIElement>().Where(x => Grid.GetColumn(x) == 0).ToList();
            var c2Items = MainGrid.Children.Cast<UIElement>().Where(x => Grid.GetColumn(x) == 1).ToList();
            c1Items.ForEach(x => Grid.SetColumn(x, 1));
            c2Items.ForEach(x => Grid.SetColumn(x, 0));

        }
    }
}
