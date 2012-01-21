using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    public class DepartmentButtonViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly DepartmentSelectorViewModel _parentViewModel;

        public DepartmentButtonViewModel(DepartmentSelectorViewModel parentViewModel,
            IApplicationState applicationState)
        {
            _parentViewModel = parentViewModel;
            _applicationState = applicationState;
            DepartmentSelectionCommand = new CaptionCommand<string>("Select", OnSelectDepartment);
        }

        private void OnSelectDepartment(string obj)
        {
            _parentViewModel.UpdateSelectedDepartment(Department);
        }

        public ICaptionCommand DepartmentSelectionCommand { get; set; }

        public string Name { get; set; }
        public Department Department { get; set; }
        public string ButtonColor { get { return _applicationState.CurrentDepartment == Department ? "Gray" : "Gainsboro"; } }

        public void Refresh()
        {
            RaisePropertyChanged(() => ButtonColor);
        }
    }
}
