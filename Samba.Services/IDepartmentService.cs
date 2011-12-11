using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface IDepartmentService : IService
    {
        void SelectDepartment(int id);
        void SelectDepartment(Department department);
        Department CurrentDepartment { get; }
        Department GetDepartment(int id);
        IEnumerable<string> GetDepartmentNames();
    }
}
