using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IDepartmentService : IService
    {
        Department GetDepartment(int id);
        IEnumerable<string> GetDepartmentNames();
        IEnumerable<Department> GetDepartments();
        int GetUserRoleCount(int departmentId);
    }
}
