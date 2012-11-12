using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.DaoClasses;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services.Implementations.DepartmentModule
{
    [Export(typeof(IDepartmentService))]
    public class DepartmentService : AbstractService, IDepartmentService
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
            _departments = null;
        }
    }
}
