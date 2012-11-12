using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services;

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
