using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.DepartmentModule
{
    /// <summary>
    /// Interaction logic for DepartmentButtonView.xaml
    /// </summary>

    [Export]
    public partial class DepartmentButtonView : UserControl
    {
        private readonly IApplicationStateSetter _applicationStateSetter;

        [ImportingConstructor]
        public DepartmentButtonView(IApplicationStateSetter applicationStateSetter, IApplicationState applicationState,
             IUserService userService)
        {
            InitializeComponent();
            _applicationStateSetter = applicationStateSetter;
            DataContext = new DepartmentButtonViewModel(applicationState, userService);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var department = ((Button)sender).DataContext as Department;
            if (department != null)
            {
                _applicationStateSetter.SetCurrentDepartment(department.Id);
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
            }
        }
    }
}
