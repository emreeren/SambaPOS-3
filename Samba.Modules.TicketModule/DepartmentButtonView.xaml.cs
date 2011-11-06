using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Modules.TicketModule
{
    /// <summary>
    /// Interaction logic for DepartmentButtonView.xaml
    /// </summary>

    [Export]
    public partial class DepartmentButtonView : UserControl
    {
        [ImportingConstructor]
        public DepartmentButtonView(TicketEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((TicketEditorViewModel)DataContext).TicketListViewModel.SelectedDepartment =
                ((Button)sender).DataContext as Department;
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
        }
    }
}
