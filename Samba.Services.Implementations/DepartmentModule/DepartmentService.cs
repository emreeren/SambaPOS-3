using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.DepartmentModule
{
    [Export(typeof(IDepartmentService))]
    public class DepartmentService : AbstractService, IDepartmentService
    {
        [ImportingConstructor]
        public DepartmentService()
        {
            ValidatorRegistry.RegisterDeleteValidator<Department>(
                x => Dao.Exists<UserRole>(y => y.DepartmentId == x.Id), Resources.Department, Resources.UserRole);
        }

        private IWorkspace _workspace;
        public IWorkspace Workspace
        {
            get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); }
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = Workspace.All<Department>().OrderBy(x => x.Order).ThenBy(x => x.Id)); }
        }

        public Department GetDepartment(int id)
        {
            return id == 0 ? null : Departments.First(x => x.Id == id);
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
