using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation.Terminal
{
    public class DepartmentSelectorViewModel : ObservableObject
    {
        public event EventHandler DepartmentSelected;

        public void InvokeDepartmentSelected(EventArgs e)
        {
            EventHandler handler = DepartmentSelected;
            if (handler != null) handler(this, e);
        }

        public IEnumerable<Department> Departments { get { return AppServices.MainDataContext.PermittedDepartments; } }
        public Department SelectedDepartment
        {
            get { return AppServices.MainDataContext.SelectedDepartment; }
            set
            {
                AppServices.MainDataContext.SelectedDepartment = value;
                RaisePropertyChanged(()=>SelectedDepartment);
            }
        }

        public DelegateCommand<Department> SelectDepartmentCommand { get; set; }

        public DepartmentSelectorViewModel()
        {
            SelectDepartmentCommand = new DelegateCommand<Department>(OnDepartmentSelected);
        }

        private void OnDepartmentSelected(Department obj)
        {
            SelectedDepartment = obj;
            InvokeDepartmentSelected(EventArgs.Empty);
        }

        public void Refresh()
        {
            RaisePropertyChanged(()=>Departments);
        }
    }
}
