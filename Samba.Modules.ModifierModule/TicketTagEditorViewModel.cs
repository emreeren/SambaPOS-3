using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Controls.UIControls;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class TicketTagEditorViewModel : ObservableObject
    {
        private static readonly AlphanumComparator AlphanumericComparator = new AlphanumComparator();
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public TicketTagEditorViewModel(IApplicationState applicationState, ICacheService cacheService, ITicketService ticketService)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            _ticketService = ticketService;

            TicketTags = new ObservableCollection<TicketTag>();
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            SelectTicketTagCommand = new DelegateCommand<TicketTag>(OnTicketTagSelected);
            UpdateFreeTagCommand = new CaptionCommand<string>(Resources.Update, OnUpdateFreeTag, CanUpdateFreeTag);
        }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                RaisePropertyChanged(() => SelectedTicket);
                SelectedTicketTagData = null;
                TicketTags.Clear();
            }
        }

        public ICaptionCommand UpdateFreeTagCommand { get; set; }
        public ICommand SelectTicketTagCommand { get; set; }
        public ICaptionCommand CloseCommand { get; set; }
        public ObservableCollection<TicketTag> TicketTags { get; set; }
        public int TagColumnCount { get { return TicketTags.Count % 7 == 0 ? TicketTags.Count / 7 : (TicketTags.Count / 7) + 1; } }
        public TicketTagGroup SelectedTicketTagData { get; set; }

        private string _freeTag;
        public string FreeTag
        {
            get { return _freeTag; }
            set
            {
                _freeTag = value;
                RaisePropertyChanged(() => FreeTag);
            }
        }

        public bool IsFreeTagEditorVisible { get; private set; }

        private bool CanUpdateFreeTag(string arg)
        {
            return !string.IsNullOrEmpty(FreeTag);
        }

        private void OnUpdateFreeTag(string obj)
        {
            var tag = new TicketTag { Name = FreeTag };
            FreeTag = string.Empty;
            UpdateTicketTag(SelectedTicket, SelectedTicketTagData, tag);
        }

        public FilteredTextBox.FilteredTextBoxType FilteredTextBoxType
        {
            get
            {
                if (SelectedTicketTagData != null)
                {
                    if (SelectedTicketTagData.IsInteger)
                        return FilteredTextBox.FilteredTextBoxType.Digits;
                    if (SelectedTicketTagData.IsDecimal)
                        return FilteredTextBox.FilteredTextBoxType.Number;
                }
                return FilteredTextBox.FilteredTextBoxType.Letters;
            }
        }

        private void OnCloseCommandExecuted(string obj)
        {
            FreeTag = string.Empty;
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnTicketTagSelected(TicketTag obj)
        {
            UpdateTicketTag(SelectedTicket, SelectedTicketTagData, obj);
        }

        private void UpdateTicketTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag)
        {
            _ticketService.UpdateTag(ticket, tagGroup, ticketTag);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        public bool TicketTagSelected(Ticket currentTicket, TicketTagGroup tagGroup)
        {
            SelectedTicket = currentTicket;

            IsFreeTagEditorVisible = tagGroup.FreeTagging;
            SelectedTicketTagData = tagGroup;
            List<TicketTag> ticketTags;

            if (IsFreeTagEditorVisible)
            {
                ticketTags = _cacheService.GetTicketTagGroupById(tagGroup.Id)
                    .TicketTags.OrderBy(x => x.Name).ToList();
            }
            else
            {
                ticketTags = _applicationState.GetTicketTagGroups().Where(
                       x => x.Name == tagGroup.Name).SelectMany(x => x.TicketTags).ToList();
            }

            if (tagGroup.FreeTagging)
                ticketTags.Sort(AlphanumericComparator);
            else
                ticketTags.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));

            TicketTags.AddRange(ticketTags);

            if (SelectedTicket.IsTaggedWith(tagGroup.Name))
            {
                if (TicketTags.Count == 1)
                {
                    UpdateTicketTag(SelectedTicket, SelectedTicketTagData, TicketTag.Empty);
                    return true;
                }
                if (!tagGroup.ForceValue) TicketTags.Add(TicketTag.Empty);
            }

            if (TicketTags.Count == 1 && !IsFreeTagEditorVisible)
            {
                UpdateTicketTag(SelectedTicket, SelectedTicketTagData, TicketTags[0]);
                return true;
            }

            RaisePropertyChanged(() => TagColumnCount);
            RaisePropertyChanged(() => IsFreeTagEditorVisible);
            RaisePropertyChanged(() => FilteredTextBoxType);
            return false;
        }
    }
}
