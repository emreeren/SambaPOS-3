using System.Collections.Generic;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface IDepartmentService 
    {
        Department GetDepartment(int id);
        IEnumerable<Department> GetDepartments();
        void UpdatePriceTag(string departmentName,string priceTag);
        void ResetCache();
    }
}
