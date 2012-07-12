using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTagValue
    {
        public string TagName { get; set; }
        public string TagNameShort { get { return string.Join("", TagName.Where(char.IsUpper)); } }
        public string TagValue { get; set; }
    }
}
