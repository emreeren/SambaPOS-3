using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.DepartmentModule.ServiceImplementations
{
    [Export(typeof(IDepartmentService))]
    public class DepartmentService : AbstractService, IDepartmentService
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public DepartmentService(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        public Department GetDepartment(int id)
        {
            return _applicationState.Departments.First(x => x.Id == id);
        }

        public IEnumerable<string> GetDepartmentNames()
        {
            return _applicationState.Departments.Select(x => x.Name);
        }

        public override void Reset()
        {
            
        }
    }
}
