using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.DepartmentModule.ServiceImplementations
{
    [Export(typeof(IDepartmentService))]
    public class DepartmentService : AbstractService, IDepartmentService
    {
        private IWorkspace _workspace;
        public IWorkspace Workspace
        {
            get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); }
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = Workspace.All<Department>()); }
        }

        public Department GetDepartment(int id)
        {
            return Departments.First(x => x.Id == id);
        }

        public IEnumerable<string> GetDepartmentNames()
        {
            return Departments.Select(x => x.Name);
        }

        public IEnumerable<Department> GetDepartments()
        {
            return Departments;
        }

        public override void Reset()
        {
            _workspace = null;
            _departments = null;
        }
    }
}
