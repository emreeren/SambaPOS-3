using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Persistance;
using Samba.Persistance.Common;
using Samba.Presentation.Common;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.PosModule
{
    public class TicketButtonViewModel : ObservableObject
    {
        private readonly OpenTicketData _openTicketData;
        private readonly int _baseEntityId;

        public DelegateCommand<string> OpenTicketCommand { get; set; }
        public DelegateCommand<string> SelectTicketCommand { get; set; }

        public TicketButtonViewModel(OpenTicketData openTicketData, Entity baseEntity)
        {
            _openTicketData = openTicketData;
            _baseEntityId = baseEntity != null ? baseEntity.Id : 0;
            OpenTicketCommand = new DelegateCommand<string>(OnOpenTicketCommand);
            SelectTicketCommand = new DelegateCommand<string>(OnSelectTicket);
        }

        public bool IsSelected { get; set; }
        public string TicketNumber { get { return _openTicketData.TicketNumber; } }
        public decimal RemainingAmount { get { return _openTicketData.RemainingAmount; } }
        public string RemainingAmountLabel { get { return RemainingAmount.ToString(LocalSettings.ReportCurrencyFormat); } }
        public IEnumerable<string> ResourceNames { get { return _openTicketData.TicketEntities.Where(x => x.EntityId != _baseEntityId).Select(x => x.EntityName); } }
        public IEnumerable<TicketTagValue> TicketTags { get { return _openTicketData.TicketTagValues; } }

        public Action SelectionChanged { get; set; }

        public string SelectionBackground { get { return IsSelected ? SystemColors.HighlightBrush.Color.ToString() : "Silver"; } }
        public string SelectionForeground { get { return IsSelected ? SystemColors.HighlightTextBrush.Color.ToString() : "Black"; } }

        public int TicketId { get { return _openTicketData.Id; } }

        private void OnSelectTicket(string obj)
        {
            IsSelected = !IsSelected;
            RaisePropertyChanged(() => SelectionBackground);
            RaisePropertyChanged(() => SelectionForeground);
            if (SelectionChanged != null) SelectionChanged.Invoke();
        }

        private void OnOpenTicketCommand(string obj)
        {
            ExtensionMethods.PublishIdEvent(_openTicketData.Id, EventTopicNames.DisplayTicket);
        }

    }
}
