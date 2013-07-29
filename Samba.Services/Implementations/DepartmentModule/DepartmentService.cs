using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Persistance;
using Samba.Persistance.Data;

namespace Samba.Services.Implementations.DepartmentModule
{
    [Export(typeof(IDepartmentService))]
    public class DepartmentService : IDepartmentService
    {
        private readonly ICacheDao _cacheDao;

        [ImportingConstructor]
        public DepartmentService(ICacheDao cacheDao)
        {
            _cacheDao = cacheDao;
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = _cacheDao.GetDepartments()); }
        }

        public Department GetDepartment(int id)
        {
            return id == 0 || Departments.All(x => x.Id != id) ? null : Departments.First(x => x.Id == id);
        }

        public IEnumerable<Department> GetDepartments()
        {
            return Departments;
        }

        public void UpdatePriceTag(string departmentName, string priceTag)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var department = workspace.Single<Department>(y => y.Name == departmentName);
                if (department != null)
                {
                    department.PriceTag = priceTag;
                    workspace.CommitChanges();
                }
            }
        }

        public void ResetCache()
        {
            _departments = null;
        }
    }
}
