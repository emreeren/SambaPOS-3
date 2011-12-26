using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.DepartmentModule
{
    [Export(typeof(IDepartmentService))]
    public class DepartmentService : AbstractService, IDepartmentService
    {
        private IWorkspace _workspace;
        public IWorkspace Workspace
        {
            get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); }
        }

        private IEnumerable<Domain.Models.Tickets.Department> _departments;
        public IEnumerable<Domain.Models.Tickets.Department> Departments
        {
            get { return _departments ?? (_departments = Workspace.All<Domain.Models.Tickets.Department>()); }
        }

        public Domain.Models.Tickets.Department GetDepartment(int id)
        {
            return Departments.First(x => x.Id == id);
        }

        public IEnumerable<string> GetDepartmentNames()
        {
            return Departments.Select(x => x.Name);
        }

        public IEnumerable<Domain.Models.Tickets.Department> GetDepartments()
        {
            return Departments;
        }

        public int GetUserRoleCount(int departmentId)
        {
           return Dao.Count<UserRole>(x => x.DepartmentId == departmentId);
        }

        public override void Reset()
        {
            _workspace = null;
            _departments = null;
        }
    }
}
