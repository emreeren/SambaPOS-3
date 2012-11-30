using System;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Modules.PosModule
{
    public class ResourceButton
    {
        private readonly Ticket _selectedTicket;
        public ResourceButton(ResourceType model, Ticket selectedTicket)
        {
            _selectedTicket = selectedTicket;
            Model = model;
        }

        public ResourceType Model { get; set; }
        public string Name
        {
            get
            {
                var format = Resources.Select_f;
                if (_selectedTicket != null && _selectedTicket.TicketResources.Any(x => x.ResourceTypeId == Model.Id))
                    format = Resources.Change_f;
                return string.Format(format, Model.EntityName).Replace(" ", Environment.NewLine);
            }
        }
    }
}