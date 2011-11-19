using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Presentation.Terminal
{
    public delegate void TagUpdatedEventHandler(TicketTagGroup item);

    public class SelectedOrderEditorViewModel : ObservableObject
    {
        public event TagUpdatedEventHandler TagUpdated;

        public OrderViewModel SelectedItem
        {
            get { return DataContext.SelectedOrder; }
            private set { DataContext.SelectedOrder = value; }
        }

        public DelegateCommand<MenuItemPortionViewModel> PortionSelectedCommand { get; set; }
        public DelegateCommand<OrderTagViewModel> PropertySelectedCommand { get; set; }
        public DelegateCommand<TicketTagViewModel> TicketTagSelectedCommand { get; set; }
        public ICaptionCommand AddTicketTagCommand { get; set; }

        public ObservableCollection<MenuItemPortionViewModel> SelectedItemPortions { get; set; }
        public ObservableCollection<OrderTagGroupViewModel> SelectedItemPropertyGroups { get; set; }
        public ObservableCollection<TicketTagViewModel> TicketTags { get; set; }

        private string _customTag;
        public string CustomTag
        {
            get { return _customTag; }
            set
            {
                _customTag = value;
                RaisePropertyChanged(() => CustomTag);
            }
        }

        public int TagColumnCount { get { return TicketTags.Count % 7 == 0 ? TicketTags.Count / 7 : (TicketTags.Count / 7) + 1; } }

        public SelectedOrderEditorViewModel()
        {
            SelectedItemPortions = new ObservableCollection<MenuItemPortionViewModel>();
            SelectedItemPropertyGroups = new ObservableCollection<OrderTagGroupViewModel>();
            TicketTags = new ObservableCollection<TicketTagViewModel>();

            PortionSelectedCommand = new DelegateCommand<MenuItemPortionViewModel>(OnPortionSelected);
            PropertySelectedCommand = new DelegateCommand<OrderTagViewModel>(OnPropertySelected);
            TicketTagSelectedCommand = new DelegateCommand<TicketTagViewModel>(OnTicketTagSelected);
            AddTicketTagCommand = new CaptionCommand<string>(Resources.AddTag, OnTicketTagAdded, CanAddTicketTag);
        }

        public void InvokeTagUpdated(TicketTagGroup item)
        {
            TagUpdatedEventHandler handler = TagUpdated;
            if (handler != null) handler(item);
        }

        private bool CanAddTicketTag(string arg)
        {
            return !string.IsNullOrEmpty(CustomTag);
        }

        private void OnTicketTagAdded(string obj)
        {
            var cachedTag = AppServices.MainDataContext.SelectedDepartment.TicketTagGroups.Single(
                x => x.Id == SelectedTicketTag.Id);
            Debug.Assert(cachedTag != null);
            var ctag = cachedTag.TicketTags.SingleOrDefault(x => x.Name.ToLower() == CustomTag.ToLower());
            if (ctag == null && cachedTag.SaveFreeTags)
            {
                using (var workspace = WorkspaceFactory.Create())
                {
                    var tt = workspace.Single<TicketTagGroup>(x => x.Id == SelectedTicketTag.Id);
                    Debug.Assert(tt != null);
                    var tag = tt.TicketTags.SingleOrDefault(x => x.Name.ToLower() == CustomTag.ToLower());
                    if (tag == null)
                    {
                        tag = new TicketTag { Name = CustomTag };
                        tt.TicketTags.Add(tag);
                        workspace.Add(tag);
                        workspace.CommitChanges();
                    }
                }
            }
            DataContext.SelectedTicket.UpdateTag(SelectedTicketTag, new TicketTag { Name = CustomTag });
            CustomTag = string.Empty;
            InvokeTagUpdated(SelectedTicketTag);
        }

        public string TicketNote
        {
            get { return DataContext.SelectedTicket != null ? DataContext.SelectedTicket.Note : ""; }
            set { DataContext.SelectedTicket.Note = value; }
        }

        public bool IsTicketNoteVisible { get { return SelectedTicketTag == null && SelectedItem == null && DataContext.SelectedTicket != null; } }
        public bool IsTagEditorVisible { get { return TicketTags.Count > 0 && SelectedItem == null && DataContext.SelectedTicket != null; } }
        public bool IsFreeTagEditorVisible { get { return SelectedTicketTag != null && SelectedTicketTag.FreeTagging; } }

        public bool IsEditorsVisible
        {
            get { return SelectedItem != null; }
        }

        public bool IsPortionsVisible
        {
            get
            {
                return SelectedItem != null
                    && !SelectedItem.IsVoided
                    && !SelectedItem.IsLocked
                    && SelectedItem.Model.PortionCount > 1;
            }
        }

        private bool _isKeyboardVisible;

        public bool IsKeyboardVisible
        {
            get { return _isKeyboardVisible; }
            set { _isKeyboardVisible = value; RaisePropertyChanged(() => IsKeyboardVisible); }
        }

        public TicketTagGroup SelectedTicketTag { get; set; }

        public void ShowKeyboard()
        {
            IsKeyboardVisible = true;
        }

        public void HideKeyboard()
        {
            IsKeyboardVisible = false;
        }

        public void Refresh(OrderViewModel order, TicketTagGroup selectedTicketTag)
        {
            HideKeyboard();
            SelectedTicketTag = null;
            SelectedItemPortions.Clear();
            SelectedItemPropertyGroups.Clear();
            TicketTags.Clear();

            SelectedItem = order;

            if (order != null)
            {
                var mi = AppServices.DataAccessService.GetMenuItem(order.Model.MenuItemId);
                if (mi.Portions.Count > 1) SelectedItemPortions.AddRange(mi.Portions.Select(x => new MenuItemPortionViewModel(x)));
                SelectedItemPropertyGroups.AddRange(
                    AppServices.MainDataContext.GetOrderTagGroupsForItem(AppServices.MainDataContext.SelectedDepartment.Id, mi).Select(x => new OrderTagGroupViewModel(x)));
            }
            else
            {
                if (selectedTicketTag != null)
                {
                    SelectedTicketTag = selectedTicketTag;

                    if (selectedTicketTag.FreeTagging)
                    {
                        TicketTags.AddRange(Dao.Query<TicketTagGroup>(x => x.Id == selectedTicketTag.Id, x => x.TicketTags).SelectMany(x => x.TicketTags).OrderBy(x => x.Name).Select(x => new TicketTagViewModel(x)));
                    }
                    else
                    {
                        TicketTags.AddRange(selectedTicketTag.TicketTags.Select(x => new TicketTagViewModel(x)));
                    }
                    RaisePropertyChanged(() => TicketTags);
                }
                else
                {
                    RaisePropertyChanged(() => TicketNote);
                    ShowKeyboard();
                }
            }

            RaisePropertyChanged(() => SelectedItem);
            RaisePropertyChanged(() => IsPortionsVisible);
            RaisePropertyChanged(() => IsEditorsVisible);
            RaisePropertyChanged(() => IsTicketNoteVisible);
            RaisePropertyChanged(() => IsTagEditorVisible);
            RaisePropertyChanged(() => IsFreeTagEditorVisible);
            RaisePropertyChanged(() => TagColumnCount);
        }

        public void CloseView()
        {
            SelectedItem = null;
            SelectedItemPortions.Clear();
            SelectedItemPropertyGroups.Clear();
            TicketTags.Clear();
            SelectedTicketTag = null;
        }

        private void OnPortionSelected(MenuItemPortionViewModel obj)
        {
            SelectedItem.UpdatePortion(obj.Model, AppServices.MainDataContext.SelectedDepartment.PriceTag);
            foreach (var model in SelectedItemPortions)
            {
                model.Refresh();
            }
        }

        private void OnPropertySelected(OrderTagViewModel obj)
        {
            var mig = SelectedItemPropertyGroups.FirstOrDefault(propertyGroup => propertyGroup.OrderTags.Contains(obj));
            Debug.Assert(mig != null);

            SelectedItem.ToggleOrderTag(mig.Model, obj.Model, AppServices.CurrentLoggedInUser.Id);

            foreach (var model in SelectedItemPropertyGroups)
            {
                model.Refresh();
            }
        }

        private void OnTicketTagSelected(TicketTagViewModel obj)
        {
            if (DataContext.SelectedTicket != null && SelectedTicketTag != null)
            {
                DataContext.SelectedTicket.UpdateTag(SelectedTicketTag, obj.Model);
                foreach (var model in TicketTags)
                {
                    model.Refresh();
                }
                InvokeTagUpdated(SelectedTicketTag);
            }
        }
    }
}
