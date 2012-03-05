using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Infrastructure.Data
{
    public interface ICacheable : IEntity
    {
        DateTime LastUpdateTime { get; set; }
    }
}
