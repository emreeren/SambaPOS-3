using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class SelectedOrdersViewModel : ObservableObject
    {
        private bool _showExtraPropertyEditor;
        private bool _showTicketNoteEditor;
        private bool _showFreeTagEditor;

        public SelectedOrdersViewModel()
        {
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            SelectReasonCommand = new DelegateCommand<int?>(OnReasonSelected);
            SelectTicketTagCommand = new DelegateCommand<TicketTag>(OnTicketTagSelected);
            PortionSelectedCommand = new DelegateCommand<MenuItemPortion>(OnPortionSelected);
            OrderTagSelectedCommand = new DelegateCommand<OrderTag>(OnOrderTagSelected);
            UpdateExtraPropertiesCommand = new CaptionCommand<string>(Resources.Update, OnUpdateExtraProperties);
            UpdateFreeTagCommand = new CaptionCommand<string>(Resources.AddAndSave, OnUpdateFreeTag, CanUpdateFreeTag);
            SelectedItemPortions = new ObservableCollection<MenuItemPortion>();
            OrderTagGroups = new ObservableCollection<OrderTagGroup>();
            Reasons = new ObservableCollection<Reason>();
            TicketTags = new ObservableCollection<TicketTag>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketViewModel>>().Subscribe(OnTicketViewModelEvent);
        }

        private void OnTicketViewModelEvent(EventParameters<TicketViewModel> obj)
        {
            if (obj.Topic == EventTopicNames.SelectTicketTag)
            {
                ResetValues(obj.Value);
                _showFreeTagEditor = SelectedTicket.LastSelectedTicketTag.FreeTagging;

                List<TicketTag> ticketTags;
                if (_showFreeTagEditor)
                {
                    ticketTags = Dao.Query<TicketTagGroup>(x => x.Id == SelectedTicket.LastSelectedTicketTag.Id,
                                                 x => x.TicketTags).SelectMany(x => x.TicketTags).OrderBy(x => x.Name).ToList();
                }
                else
                {
                    ticketTags = AppServices.MainDataContext.SelectedDepartment.TicketTagGroups.Where(
                           x => x.Name == obj.Value.LastSelectedTicketTag.Name).SelectMany(x => x.TicketTags).ToList();
                }
                ticketTags.Sort(new AlphanumComparator());
                TicketTags.AddRange(ticketTags);

                if (SelectedTicket.IsTaggedWith(SelectedTicket.LastSelectedTicketTag.Name)) TicketTags.Add(TicketTag.Empty);
                if (TicketTags.Count == 1 && !_showFreeTagEditor) obj.Value.UpdateTag(SelectedTicket.LastSelectedTicketTag, TicketTags[0]);
                RaisePropertyChanged(() => TagColumnCount);
                RaisePropertyChanged(() => IsFreeTagEditorVisible);
                RaisePropertyChanged(() => FilteredTextBoxType);
            }

            if (obj.Topic == EventTopicNames.SelectVoidReason)
            {
                ResetValues(obj.Value);
                Reasons.AddRange(AppServices.MainDataContext.Reasons.Values.Where(x => x.ReasonType == 0));
                if (Reasons.Count == 0) obj.Value.VoidSelectedItems(0);
                RaisePropertyChanged(() => ReasonColumnCount);
            }

            if (obj.Topic == EventTopicNames.SelectGiftReason)
            {
                ResetValues(obj.Value);
                Reasons.AddRange(AppServices.MainDataContext.Reasons.Values.Where(x => x.ReasonType == 1));
                if (Reasons.Count == 0) obj.Value.GiftSelectedItems(0);
                RaisePropertyChanged(() => ReasonColumnCount);
            }

            if (obj.Topic == EventTopicNames.SelectExtraProperty)
            {
                ResetValues(obj.Value);
                _showExtraPropertyEditor = true;
                RaisePropertyChanged(() => IsExtraPropertyEditorVisible);
                RaisePropertyChanged(() => IsPortionsVisible);
            }

            if (obj.Topic == EventTopicNames.EditTicketNote)
            {
                ResetValues(obj.Value);
                _showTicketNoteEditor = true;
                RaisePropertyChanged(() => IsTicketNoteEditorVisible);
            }
        }

        private void ResetValues(TicketViewModel selectedTicket)
        {
            SelectedTicket = null;
            SelectedItem = null;
            SelectedItemPortions.Clear();
            OrderTagGroups.Clear();
            Reasons.Clear();
            TicketTags.Clear();
            _showExtraPropertyEditor = false;
            _showTicketNoteEditor = false;
            _showFreeTagEditor = false;
            SetSelectedTicket(selectedTicket);
        }

        public TicketViewModel SelectedTicket { get; private set; }
        public TicketItemViewModel SelectedItem { get; private set; }

        public ICaptionCommand CloseCommand { get; set; }
        public ICaptionCommand UpdateExtraPropertiesCommand { get; set; }
        public ICaptionCommand UpdateFreeTagCommand { get; set; }
        public ICommand SelectReasonCommand { get; set; }
        public ICommand SelectTicketTagCommand { get; set; }

        public DelegateCommand<MenuItemPortion> PortionSelectedCommand { get; set; }
        public ObservableCollection<MenuItemPortion> SelectedItemPortions { get; set; }

        public DelegateCommand<OrderTag> OrderTagSelectedCommand { get; set; }
        public ObservableCollection<OrderTagGroup> OrderTagGroups { get; set; }

        public ObservableCollection<Reason> Reasons { get; set; }
        public ObservableCollection<TicketTag> TicketTags { get; set; }

        public int ReasonColumnCount { get { return Reasons.Count % 7 == 0 ? Reasons.Count / 7 : (Reasons.Count / 7) + 1; } }
        public int TagColumnCount { get { return TicketTags.Count % 7 == 0 ? TicketTags.Count / 7 : (TicketTags.Count / 7) + 1; } }

        public FilteredTextBox.FilteredTextBoxType FilteredTextBoxType
        {
            get
            {
                if (SelectedTicket != null && SelectedTicket.LastSelectedTicketTag != null)
                {
                    if (SelectedTicket.LastSelectedTicketTag.IsInteger)
                        return FilteredTextBox.FilteredTextBoxType.Digits;
                    if (SelectedTicket.LastSelectedTicketTag.IsDecimal)
                        return FilteredTextBox.FilteredTextBoxType.Number;
                }
                return FilteredTextBox.FilteredTextBoxType.Letters;
            }
        }

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

        public bool IsPortionsVisible
        {
            get
            {
                return SelectedItem != null
                    && Reasons.Count == 0
                    && !SelectedItem.IsVoided
                    && !SelectedItem.IsLocked
                    && SelectedItemPortions.Count > 0;
            }
        }

        private void OnCloseCommandExecuted(string obj)
        {
            _showTicketNoteEditor = false;
            _showExtraPropertyEditor = false;
            _showFreeTagEditor = false;
            FreeTag = string.Empty;
            SelectedTicket.ClearSelectedItems();
        }

        public bool IsFreeTagEditorVisible { get { return _showFreeTagEditor; } }
        public bool IsExtraPropertyEditorVisible { get { return _showExtraPropertyEditor && SelectedItem != null; } }
        public bool IsTicketNoteEditorVisible { get { return _showTicketNoteEditor; } }


        private bool CanUpdateFreeTag(string arg)
        {
            return !string.IsNullOrEmpty(FreeTag);
        }

        private void OnUpdateFreeTag(string obj)
        {
            var cachedTag = AppServices.MainDataContext.SelectedDepartment.TicketTagGroups.Single(
                x => x.Id == SelectedTicket.LastSelectedTicketTag.Id);
            Debug.Assert(cachedTag != null);
            var ctag = cachedTag.TicketTags.SingleOrDefault(x => x.Name.ToLower() == FreeTag.ToLower());
            if (ctag == null && cachedTag.SaveFreeTags)
            {
                using (var workspace = WorkspaceFactory.Create())
                {
                    var tt = workspace.Single<TicketTagGroup>(x => x.Id == SelectedTicket.LastSelectedTicketTag.Id);
                    Debug.Assert(tt != null);
                    var tag = tt.TicketTags.SingleOrDefault(x => x.Name.ToLower() == FreeTag.ToLower());
                    if (tag == null)
                    {
                        tag = new TicketTag { Name = FreeTag };
                        tt.TicketTags.Add(tag);
                        workspace.Add(tag);
                        workspace.CommitChanges();
                    }
                }
            }
            SelectedTicket.UpdateTag(SelectedTicket.LastSelectedTicketTag, new TicketTag { Name = FreeTag });
            FreeTag = string.Empty;
        }

        private void OnUpdateExtraProperties(string obj)
        {
            SelectedTicket.RefreshVisuals();
            _showExtraPropertyEditor = false;
            RaisePropertyChanged(() => IsExtraPropertyEditorVisible);
        }

        private void OnTicketTagSelected(TicketTag obj)
        {
            SelectedTicket.UpdateTag(SelectedTicket.LastSelectedTicketTag, obj);
        }

        private void OnReasonSelected(int? reasonId)
        {
            var rid = reasonId.GetValueOrDefault(0);
            Reason r = AppServices.MainDataContext.Reasons[rid];
            if (r.ReasonType == 0)
                SelectedTicket.VoidSelectedItems(rid);
            if (r.ReasonType == 1)
                SelectedTicket.GiftSelectedItems(rid);
        }

        private void OnPortionSelected(MenuItemPortion obj)
        {
            SelectedItem.UpdatePortion(obj, AppServices.MainDataContext.SelectedDepartment.PriceTag);
            if (OrderTagGroups.Count == 0)
                SelectedTicket.ClearSelectedItems();
        }

        private void OnOrderTagSelected(OrderTag orderTag)
        {
            var mig = OrderTagGroups.FirstOrDefault(propertyGroup => propertyGroup.OrderTags.Contains(orderTag));
            Debug.Assert(mig != null);
            SelectedItem.ToggleProperty(mig, orderTag);
            SelectedTicket.RefreshVisuals();
        }

        private void SetSelectedTicket(TicketViewModel ticketViewModel)
        {
            SelectedTicket = ticketViewModel;
            SelectedItem = SelectedTicket.SelectedItems.Count() == 1 ? SelectedTicket.SelectedItems[0] : null;
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => SelectedItem);
            RaisePropertyChanged(() => IsTicketNoteEditorVisible);
            RaisePropertyChanged(() => IsExtraPropertyEditorVisible);
            RaisePropertyChanged(() => IsFreeTagEditorVisible);
            RaisePropertyChanged(() => IsPortionsVisible);
        }

        public bool ShouldDisplay(TicketViewModel value)
        {
            ResetValues(value);

            if (SelectedItem == null || SelectedItem.Model.Locked) return false;

            if (SelectedTicket != null && !SelectedItem.Model.Voided && !SelectedItem.Model.Locked)
            {
                var id = SelectedItem.Model.MenuItemId;

                var mi = AppServices.DataAccessService.GetMenuItem(id);
                if (SelectedItem.Model.PortionCount > 1) SelectedItemPortions.AddRange(mi.Portions);
                OrderTagGroups.AddRange(AppServices.MainDataContext.GetOrderTagGroupsForItem(value.Model.DepartmentId, mi));
                RaisePropertyChanged(() => IsPortionsVisible);
            }

            return SelectedItemPortions.Count > 1 || OrderTagGroups.Count > 0;
        }

    }
}
