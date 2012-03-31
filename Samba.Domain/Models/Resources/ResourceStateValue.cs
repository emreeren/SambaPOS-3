using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceStateValue : Value
    {
        public int ResoruceId { get; set; }
        public DateTime Date { get; set; }
        public int StateId { get; set; }
    }
}
