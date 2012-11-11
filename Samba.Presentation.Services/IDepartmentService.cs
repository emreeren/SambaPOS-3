using System.Collections.Generic;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IDepartmentService : IPresentationService
    {
        Department GetDepartment(int id);
        IEnumerable<string> GetDepartmentNames();
        IEnumerable<Department> GetDepartments();
    }
}
