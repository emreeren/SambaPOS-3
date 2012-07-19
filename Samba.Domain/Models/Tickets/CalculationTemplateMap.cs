using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class CalculationTemplateMap : Value, IAbstractMapModel
    {
        public int TerminalId { get; set; }
        public int CalculationTemplateId { get; set; }
        public int DepartmentId { get; set; }
        public int UserRoleId { get; set; }
    }
}
