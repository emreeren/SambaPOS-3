using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Modules.DepartmentModule.ServiceImplementations
{
    [Export(typeof(IDepartmentService))]
    public class DepartmentService : IDepartmentService
    {
        private IWorkspace _workspace;
        public IWorkspace Workspace
        {
            get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); }
        }

        public void SelectDepartment(int id)
        {
            SelectDepartment(Departments.SingleOrDefault(x => x.Id == id));
        }

        public void SelectDepartment(Department department)
        {
            CurrentDepartment = department;
        }

        public Department CurrentDepartment { get; private set; }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = Workspace.All<Department>()); }
        }

        private IEnumerable<Department> _permittedDepartments;
        public IEnumerable<Department> PermittedDepartments
        {
            get
            {
                return _permittedDepartments ?? (
                    _permittedDepartments = Departments.Where(
                      x => AppServices.IsUserPermittedFor(PermissionNames.UseDepartment + x.Id)));
            }
        }

        public Department GetDepartment(int id)
        {
            return Departments.First(x => x.Id == id);
        }

        public IEnumerable<string> GetDepartmentNames()
        {
            return Departments.Select(x => x.Name);
        }

        public void Reset()
        {
            _departments = null;
            _permittedDepartments = null;
        }
    }
}
