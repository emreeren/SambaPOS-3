using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    public class TicketButtonViewModel : ObservableObject
    {
        private readonly OpenTicketData _openTicketData;
        private readonly int _baseResourceId;

        public TicketButtonViewModel(OpenTicketData openTicketData, Resource baseResource)
        {
            _openTicketData = openTicketData;
            _baseResourceId = baseResource != null ? baseResource.Id : 0;
            OpenTicketCommand = new DelegateCommand<string>(OnOpenTicketCommand);
        }

        private void OnOpenTicketCommand(string obj)
        {
            ExtensionMethods.PublishIdEvent(_openTicketData.Id, EventTopicNames.DisplayTicket);
        }

        public DelegateCommand<string> OpenTicketCommand { get; set; }
        public string TicketNumber { get { return _openTicketData.TicketNumber; } }
        public decimal RemainingAmount { get { return _openTicketData.RemainingAmount; } }
        public string RemainingAmountLabel { get { return RemainingAmount.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public IEnumerable<string> ResourceNames { get { return _openTicketData.TicketResources.Where(x => x.ResourceId != _baseResourceId).Select(x => x.ResourceName); } }
        public IEnumerable<TicketTagValue> TicketTags { get { return _openTicketData.TicketTagValues; } }
    }
}
