using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    internal enum TagTypes
    {
        Alphanumeric,
        Numeric,
        Price
    }

    public class TicketTagGroup : IEntity, IOrderable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }

        public virtual Numerator Numerator { get; set; }
        private IList<TicketTag> _ticketTags;

        public int Action { get; set; }
        public bool FreeTagging { get; set; }
        public bool SaveFreeTags { get; set; }
        public string ButtonColorWhenTagSelected { get; set; }
        public string ButtonColorWhenNoTagSelected { get; set; }
        public bool ActiveOnPosClient { get; set; }
        public bool ActiveOnTerminalClient { get; set; }
        public bool ForceValue { get; set; }
        public int DataType { get; set; }

        public bool IsNumeric { get { return IsDecimal || IsInteger; } }
        public bool IsAlphanumeric { get { return DataType == 0; } }
        public bool IsInteger { get { return DataType == 1; } }
        public bool IsDecimal { get { return DataType == 2; } }

        public string UserString
        {
            get { return Name; }
        }

        public virtual IList<TicketTag> TicketTags
        {
            get { return _ticketTags; }
            set { _ticketTags = value; }
        }

        public TicketTagGroup()
        {
            _ticketTags = new List<TicketTag>();
            ButtonColorWhenNoTagSelected = "Gainsboro";
            ButtonColorWhenTagSelected = "Gainsboro";
            ActiveOnPosClient = true;
            SaveFreeTags = true;
        }
    }
}
