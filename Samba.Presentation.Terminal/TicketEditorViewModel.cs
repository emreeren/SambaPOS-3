using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using System.Linq;

namespace Samba.Presentation.Terminal
{
    public delegate void TicketItemSelectedEventHandler(TicketItemViewModel item);

    public class TicketEditorViewModel : ObservableObject
    {
        public event EventHandler OnAddMenuItemsRequested;
        public event EventHandler OnCloseTicketRequested;
        public event EventHandler OnSelectTableRequested;
        public event EventHandler OnTicketNoteEditorRequested;
        public event EventHandler OnTicketTagEditorRequested;

        public void InvokeOnTicketTagEditorRequested(TicketTagGroup tag)
        {
            EventHandler handler = OnTicketTagEditorRequested;
            if (handler != null) handler(tag, EventArgs.Empty);
        }

        public void InvokeOnTicketNoteEditorRequested(EventArgs e)
        {
            EventHandler handler = OnTicketNoteEditorRequested;
            if (handler != null) handler(this, e);
        }

        public void InvokeOnSelectTableRequested(EventArgs e)
        {
            EventHandler handler = OnSelectTableRequested;
            if (handler != null) handler(this, e);
        }

        public void InvokeCloseTicketRequested(EventArgs e)
        {
            EventHandler handler = OnCloseTicketRequested;
            if (handler != null) handler(this, e);
        }

        public void InvokeOnAddMenuItemsRequested(EventArgs e)
        {
            EventHandler handler = OnAddMenuItemsRequested;
            if (handler != null) handler(this, e);
        }

        public TicketViewModel SelectedTicket
        {
            get { return DataContext.SelectedTicket; }
        }

        private string _selectedTicketTitle;
        public string SelectedTicketTitle
        {
            get { return _selectedTicketTitle; }
            set { _selectedTicketTitle = value; RaisePropertyChanged(() => SelectedTicketTitle); }
        }

        public bool IsTicketTotalVisible
        {
            get { return SelectedTicket != null && SelectedTicket.IsTicketTotalVisible; }
        }

        public bool IsTicketPaymentVisible
        {
            get { return SelectedTicket != null && SelectedTicket.IsTicketPaymentVisible; }
        }

        public bool IsTicketRemainingVisible
        {
            get { return SelectedTicket != null && SelectedTicket.IsTicketRemainingVisible; }
        }

        public bool IsTicketDiscountVisible
        {
            get { return SelectedTicket != null && SelectedTicket.IsTicketDiscountVisible; }
        }

        private bool? _isTableButtonVisible;
        public bool? IsTableButtonVisible
        {
            get
            {
                return _isTableButtonVisible ??
                       (_isTableButtonVisible =
                        AppServices.MainDataContext.SelectedDepartment != null &&
                        AppServices.MainDataContext.SelectedDepartment.TerminalTableScreenId > 0);
            }
        }

        public string Note
        {
            get
            {
                if (SelectedTicket != null)
                {
                    var result = SelectedTicket.Note;
                    if (SelectedTicket.IsTagged)
                        if (!string.IsNullOrEmpty(result))
                            result = "Not: " + result + "\r";
                    result += SelectedTicket.TagDisplay;
                    return result;
                }
                return "";
            }
        }
        public bool IsTicketNoteVisible { get { return SelectedTicket != null && (SelectedTicket.IsTicketNoteVisible || SelectedTicket.IsTagged); } }

        public TicketItemViewModel LastSelectedTicketItem { get; set; }

        public CaptionCommand<string> AddMenuItemsCommand { get; set; }
        public CaptionCommand<string> PrintTicketCommand { get; set; }
        public CaptionCommand<string> DeleteSelectedItemsCommand { get; set; }
        public CaptionCommand<string> ChangeTableCommand { get; set; }
        public CaptionCommand<string> IncSelectedQuantityCommand { get; set; }
        public CaptionCommand<string> DecSelectedQuantityCommand { get; set; }
        public CaptionCommand<string> MoveSelectedItemsCommand { get; set; }
        public CaptionCommand<string> EditTicketNoteCommand { get; set; }
        public CaptionCommand<PrintJob> PrintJobCommand { get; set; }
        public CaptionCommand<TicketTagGroup> TicketTagCommand { get; set; }

        public IEnumerable<PrintJobButton> PrintJobButtons
        {
            get
            {
                return SelectedTicket != null
                    ? SelectedTicket.PrintJobButtons.Where(x => x.Model.UseFromTerminal)
                    : null;
            }
        }

        public IEnumerable<TicketTagButton> TicketTagButtons
        {
            get
            {
                return SelectedTicket != null
                    ? AppServices.MainDataContext.SelectedDepartment.TicketTagGroups
                        .Where(x => x.ActiveOnTerminalClient)
                        .OrderBy(x => x.Order)
                        .Select(x => new TicketTagButton(x, SelectedTicket))
                        : null;
            }
        }

        public TicketEditorViewModel()
        {
            AddMenuItemsCommand = new CaptionCommand<string>(Resources.Add, OnAddMenuItems, CanAddMenuItems);
            DeleteSelectedItemsCommand = new CaptionCommand<string>(Resources.Delete_ab, OnDeleteSelectedItems, CanDeleteSelectedItems);
            ChangeTableCommand = new CaptionCommand<string>(Resources.Table, OnChangeTable, CanChangeTable);
            IncSelectedQuantityCommand = new CaptionCommand<string>("+", OnIncSelectedQuantity, CanIncSelectedQuantity);
            DecSelectedQuantityCommand = new CaptionCommand<string>("-", OnDecSelectedQuantity, CanDecSelectedQuantity);
            MoveSelectedItemsCommand = new CaptionCommand<string>(Resources.Divide_ab, OnMoveSelectedItems, CanMoveSelectedItems);
            EditTicketNoteCommand = new CaptionCommand<string>("Not", OnEditTicketNote);
            PrintJobCommand = new CaptionCommand<PrintJob>(Resources.Print_ab, OnPrintJobExecute, CanExecutePrintJob);
            TicketTagCommand = new CaptionCommand<TicketTagGroup>("Tag", OnTicketTagExecute, CanTicketTagExecute);

            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketItemViewModel>>().Subscribe(
                x =>
                {
                    if (SelectedTicket != null && x.Topic == EventTopicNames.SelectedItemsChanged)
                    {
                        LastSelectedTicketItem = x.Value.Selected ? x.Value : null;
                        foreach (var item in SelectedTicket.SelectedItems)
                        { item.IsLastSelected = item == LastSelectedTicketItem; }
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
             x =>
             {
                 if (x.Topic == EventTopicNames.RefreshSelectedTicket)
                 {
                     DataContext.RefreshSelectedTicket();
                     Refresh();
                 }
             });
        }

        private bool CanExecutePrintJob(PrintJob arg)
        {
            return arg != null && SelectedTicket != null && (!SelectedTicket.IsLocked || SelectedTicket.Model.GetPrintCount(arg.Id) == 0);
        }

        private void OnPrintJobExecute(PrintJob printJob)
        {
            var message = SelectedTicket.GetPrintError();

            if (!string.IsNullOrEmpty(message))
            {
                MainWindowViewModel.ShowFeedback(message);
                return;
            }

            if (SelectedTicket.Id == 0)
            {
                var result = DataContext.CloseSelectedTicket();
                DataContext.OpenTicket(result.TicketId);
            }
            AppServices.PrintService.ManualPrintTicket(SelectedTicket.Model, printJob);
            InvokeCloseTicketRequested(EventArgs.Empty);
        }

        private bool CanTicketTagExecute(TicketTagGroup arg)
        {
            return SelectedTicket != null && (!SelectedTicket.IsLocked || !SelectedTicket.IsTaggedWith(arg.Name));
        }

        private void OnTicketTagExecute(TicketTagGroup obj)
        {
            InvokeOnTicketTagEditorRequested(obj);
        }

        private void OnEditTicketNote(string obj)
        {
            InvokeOnTicketNoteEditorRequested(EventArgs.Empty);
        }

        private void OnMoveSelectedItems(string obj)
        {
            InvokeOnSelectTableRequested(EventArgs.Empty);
        }

        private bool CanMoveSelectedItems(string arg)
        {
            return SelectedTicket != null && SelectedTicket.SelectedItems.Count > 0 && SelectedTicket.CanMoveSelectedItems();
        }

        private bool CanDecSelectedQuantity(string arg)
        {
            return LastSelectedTicketItem != null &&
                LastSelectedTicketItem.Quantity > 1 &&
                !LastSelectedTicketItem.IsGifted &&
                !LastSelectedTicketItem.IsVoided;
        }

        private void OnDecSelectedQuantity(string obj)
        {
            if (LastSelectedTicketItem.IsLocked)
                LastSelectedTicketItem.DecSelectedQuantity();
            else LastSelectedTicketItem.Quantity--;
        }

        private bool CanIncSelectedQuantity(string arg)
        {
            return LastSelectedTicketItem != null &&
               (LastSelectedTicketItem.Quantity > 1 || !LastSelectedTicketItem.IsLocked) &&
               !LastSelectedTicketItem.IsGifted &&
               !LastSelectedTicketItem.IsVoided;
        }

        private void OnIncSelectedQuantity(string obj)
        {
            if (LastSelectedTicketItem.IsLocked)
                LastSelectedTicketItem.IncSelectedQuantity();
            else LastSelectedTicketItem.Quantity++;
        }

        private bool CanChangeTable(string arg)
        {
            if (SelectedTicket != null && !SelectedTicket.IsLocked)
                return SelectedTicket.CanChangeTable();
            return true;
        }

        private void OnChangeTable(string obj)
        {
            SelectedTicket.ClearSelectedItems();
            InvokeOnSelectTableRequested(EventArgs.Empty);
        }

        private bool CanDeleteSelectedItems(string arg)
        {
            return SelectedTicket != null && SelectedTicket.CanCancelSelectedItems();
        }

        private void OnDeleteSelectedItems(string obj)
        {
            SelectedTicket.CancelSelectedItems();
        }

        public void UpdateSelectedTicketTitle()
        {
            SelectedTicketTitle = SelectedTicket == null ? Resources.NewTicket : SelectedTicket.Title;
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => IsTicketRemainingVisible);
            RaisePropertyChanged(() => IsTicketPaymentVisible);
            RaisePropertyChanged(() => IsTicketTotalVisible);
            RaisePropertyChanged(() => IsTicketDiscountVisible);
            RaisePropertyChanged(() => PrintJobButtons);
            RaisePropertyChanged(() => TicketTagButtons);
            RaisePropertyChanged(() => Note);
            RaisePropertyChanged(() => IsTicketNoteVisible);
            RaisePropertyChanged(() => IsTableButtonVisible);
            UpdateSelectedTicketTitle();
        }

        private void OnAddMenuItems(string obj)
        {
            InvokeOnAddMenuItemsRequested(EventArgs.Empty);
        }

        private bool CanAddMenuItems(string arg)
        {
            if (SelectedTicket != null && SelectedTicket.IsLocked)
                return AppServices.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets);
            return true;
        }

        public void ResetCache()
        {
            _isTableButtonVisible = null;
        }
    }
}
