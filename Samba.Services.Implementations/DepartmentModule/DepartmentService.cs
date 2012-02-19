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
            ValidatorRegistry.RegisterDeleteValidator(new DepartmentDeleteValidator());
        }

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
            if (id == 0) return null;
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

    public class DepartmentDeleteValidator : SpecificationValidator<Department>
    {
        public override string GetErrorMessage(Department model)
        {
            if (Dao.Exists<UserRole>(x => x.DepartmentId == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Department, Resources.UserRole);
            return "";
        }
    }
}
