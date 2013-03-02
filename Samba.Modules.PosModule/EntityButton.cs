using System;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Modules.PosModule
{
    public class EntityButton
    {
        private readonly Ticket _selectedTicket;
        public EntityButton(EntityType model, Ticket selectedTicket)
        {
            _selectedTicket = selectedTicket;
            Model = model;
        }

        public EntityType Model { get; set; }
        public string Name
        {
            get
            {
                var format = Resources.Select_f;
                if (_selectedTicket != null && _selectedTicket.TicketEntities.Any(x => x.EntityTypeId == Model.Id))
                    format = Resources.Change_f;
                return string.Format(format, Model.EntityName).Replace(" ", Environment.NewLine);
            }
        }
    }
}