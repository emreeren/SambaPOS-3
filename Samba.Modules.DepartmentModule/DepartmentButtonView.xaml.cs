using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    /// <summary>
    /// Interaction logic for DepartmentButtonView.xaml
    /// </summary>

    [Export]
    public partial class DepartmentButtonView : UserControl
    {
        private IDepartmentService _departmentService;

        [ImportingConstructor]
        public DepartmentButtonView(IDepartmentService departmentService, ITicketService ticketService, IWorkPeriodService workPeriodService)
        {
            InitializeComponent();
            _departmentService = departmentService;
            DataContext = new DepartmentButtonViewModel(departmentService, ticketService, workPeriodService);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _departmentService.SelectDepartment(((Button)sender).DataContext as Department);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
        }
    }
}
