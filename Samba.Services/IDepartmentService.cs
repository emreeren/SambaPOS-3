﻿using System.Collections.Generic;
using Samba.Domain.Models.Tickets;

namespace Samba.Services
{
    public interface IDepartmentService 
    {
        Department GetDepartment(int id);
        IEnumerable<string> GetDepartmentNames();
        IEnumerable<Department> GetDepartments();
        void ResetCache();
    }
}
