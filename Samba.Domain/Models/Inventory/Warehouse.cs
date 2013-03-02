﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class Warehouse : EntityClass, IOrderable
    {
        public int WarehouseTypeId { get; set; }
        public int SortOrder { get; set; }
        public string UserString { get { return Name; } }
    }
}
