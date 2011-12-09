using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface IDepartmentService:IService
    {
        void SelectDepartment(Department department);
        void SelectDepartment(int id);
        Department CurrentDepartment { get; }
        Department GetDepartment(int id);
        IEnumerable<string> GetDepartmentNames();
    }
}
