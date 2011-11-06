using System;
using System.Diagnostics;
using System.Windows;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure;
using Samba.Infrastructure.Settings;
using Samba.Localization.Engine;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;
using System.Linq;

namespace Samba.Presentation.Terminal
{
    public class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            LocalizeDictionary.ChangeLanguage(LocalSettings.CurrentLanguage);

            LocalSettings.DefaultCurrencyFormat = "#,#0.00";
            LocalSettings.AppPath = System.IO.Path.GetDirectoryName(Application.ResourceAssembly.Location);
            AppServices.MainDispatcher = Application.Current.Dispatcher;
            GenericRuleRegistator.RegisterOnce();
            TriggerService.UpdateCronObjects();

            LoggedInUserViewModel = new LoggedInUserViewModel();
            LoggedInUserViewModel.CloseButtonClickedEvent += LoggedInUserViewModelCloseButtonClickedEvent;

            LoginViewModel = new LoginViewModel();
            LoginViewModel.PinSubmitted += LoginViewModelPinSubmitted;

            TableScreenViewModel = new TableScreenViewModel();
            TableScreenViewModel.TableSelectedEvent += TableScreenViewModelTableSelectedEvent;

            TicketScreenViewModel = new TicketScreenViewModel();
            TicketScreenViewModel.TicketSelectedEvent += TicketScreenViewModelTicketSelectedEvent;

            DepartmentSelectorViewModel = new DepartmentSelectorViewModel();
            DepartmentSelectorViewModel.DepartmentSelected += DepartmentSelectorViewModelDepartmentSelected;

            TicketEditorViewModel = new TicketEditorViewModel();
            TicketEditorViewModel.OnAddMenuItemsRequested += TicketEditorViewModel_OnAddMenuItemsRequested;
            TicketEditorViewModel.OnCloseTicketRequested += TicketEditorViewModel_OnCloseTicketRequested;
            TicketEditorViewModel.OnSelectTableRequested += TicketEditorViewModel_OnSelectTableRequested;
            TicketEditorViewModel.OnTicketNoteEditorRequested += TicketEditorViewModel_OnTicketNoteEditorRequested;
            TicketEditorViewModel.OnTicketTagEditorRequested += TicketEditorViewModel_OnTicketTagEditorRequested;

            MenuItemSelectorViewModel = new MenuItemSelectorViewModel();
            MenuItemSelectorViewModel.OnTicketItemSelected += MenuItemSelectorViewModel_OnTicketItemSelected;

            SelectedTicketItemEditorViewModel = new SelectedTicketItemEditorViewModel();
            SelectedTicketItemEditorViewModel.TagUpdated += SelectedTicketItemEditorViewModelTagUpdated;

            PermissionRegistry.RegisterPermission(PermissionNames.AddItemsToLockedTickets, PermissionCategories.Ticket, "Kilitli adisyona ekleme yapabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.GiftItems, PermissionCategories.Ticket, "İkram yapabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.VoidItems, PermissionCategories.Ticket, "İade alabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.MoveTicketItems, PermissionCategories.Ticket, "Adisyon satırlarını taşıyabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.MoveUnlockedTicketItems, PermissionCategories.Ticket, "Kilitlenmemiş adisyon satırlarını taşıyabilir");
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeExtraProperty, PermissionCategories.Ticket, "Ekstra özellik girebilir");

            AppServices.MessagingService.RegisterMessageListener(new MessageListener());
            if (LocalSettings.StartMessagingClient)
                AppServices.MessagingService.StartMessagingClient();

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
                x =>
                {
                    if (SelectedIndex == 2 && x.Topic == EventTopicNames.MessageReceivedEvent
                        && x.Value.Command == Messages.TicketRefreshMessage)
                    {
                        TableScreenViewModel.Refresh();
                    }
                });
        }

        void SelectedTicketItemEditorViewModelTagUpdated(TicketTagGroup item)
        {
            Debug.Assert(DataContext.SelectedTicket != null);
            if (DataContext.SelectedTicket.Items.Count == 0)
            {
                ActivateMenuItemSelector();
            }
            else if (item.Action > 0)
            {
                CloseSelectedTicket();
                ActivateTableView();
            }
            else
            {
                ActivateTicketView(null);
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(()=>SelectedIndex);
                RaisePropertyChanged(()=>IsLoggedUserVisible);
            }
        }

        private int _selectedTicketViewIndex;
        public int SelectedTicketViewIndex
        {
            get { return _selectedTicketViewIndex; }
            set
            {
                _selectedTicketViewIndex = value;
                RaisePropertyChanged(()=>SelectedTicketViewIndex);
            }
        }

        public bool IsLoggedUserVisible
        {
            get
            {
                return SelectedIndex != 0;
            }
        }

        public LoggedInUserViewModel LoggedInUserViewModel { get; set; }
        public LoginViewModel LoginViewModel { get; set; }
        public TableScreenViewModel TableScreenViewModel { get; set; }
        public TicketScreenViewModel TicketScreenViewModel { get; set; }
        public DepartmentSelectorViewModel DepartmentSelectorViewModel { get; set; }
        public TicketEditorViewModel TicketEditorViewModel { get; set; }
        public MenuItemSelectorViewModel MenuItemSelectorViewModel { get; set; }
        public SelectedTicketItemEditorViewModel SelectedTicketItemEditorViewModel { get; set; }

        void LoginViewModelPinSubmitted(object sender, string pinValue)
        {
            if (pinValue == "065058")
            {
                Application.Current.Shutdown();
            }

            var user = AppServices.LoginUser(pinValue);
            LoggedInUserViewModel.Refresh();
            if (user != User.Nobody)
            {
                if (user.UserRole.DepartmentId != 0 && !AppServices.IsUserPermittedFor(PermissionNames.ChangeDepartment))
                {
                    AppServices.MainDataContext.SelectedDepartment =
                        AppServices.MainDataContext.Departments.Single(x => x.Id == user.UserRole.DepartmentId);
                    ActivateTableView();
                }
                else if (AppServices.MainDataContext.PermittedDepartments.Count() == 1)
                {
                    AppServices.MainDataContext.SelectedDepartment =
                        AppServices.MainDataContext.PermittedDepartments.First();
                    ActivateTableView();
                }
                else ActivateDepartmentSelector();
            }
            TicketEditorViewModel.ResetCache();
        }

        void TicketScreenViewModelTicketSelectedEvent(int selectedTicketId)
        {
            if (!AppServices.MainDataContext.IsCurrentWorkPeriodOpen)
            {
                ShowFeedback("Gün Sonu yapıldığı için işlem yapamazsınız.");
                return;
            }

            DataContext.OpenTicket(selectedTicketId);
            if (selectedTicketId > 0)
            {
                ActivateTicketView(null);
            }
            else
            {
                ActivateMenuItemSelector();
            }

        }

        void TableScreenViewModelTableSelectedEvent(Table selectedTable)
        {
            //Id #10: Gün sonu yapıldıysa iptal et.
            if (!AppServices.MainDataContext.IsCurrentWorkPeriodOpen)
            {
                ShowFeedback("Gün Sonu yapıldığı için işlem yapamazsınız.");
                return;
            }

            if (DataContext.SelectedTicket != null)
            {
                if (DataContext.SelectedTicket.SelectedItems.Count == 0)
                {
                    TicketViewModel.AssignLocationToSelectedTicket(selectedTable.Id);
                    //AppServices.MainDataContext.AssignTableToSelectedTicket(selectedTable.Id);
                    ShowFeedback("Adisyon " + selectedTable.Name + " masasına taşındı.");
                }
                else
                {
                    MoveSelectedItems(selectedTable.Id);
                    ShowFeedback("Seçili ürünler " + selectedTable.Name + " masasına taşındı.");
                }
                CloseSelectedTicket();
                ActivateTableView();
            }
            else ActivateTicketView(selectedTable);
        }

        void LoggedInUserViewModelCloseButtonClickedEvent(object sender, EventArgs e)
        {
            if (SelectedIndex == 5)
            {
                if (SelectedTicketItemEditorViewModel.SelectedTicketTag != null && DataContext.SelectedTicket.Items.Count == 0)
                {
                    DataContext.CloseSelectedTicket();
                    ActivateTableView();
                }
                else if (MenuItemSelectorViewModel.AddedMenuItems.Count > 0)
                    SelectedIndex = 4;
                else
                    ActivateTicketView(null);

                SelectedTicketItemEditorViewModel.CloseView();
            }
            else if (SelectedIndex == 4)
            {
                if (DataContext.SelectedTicket.Items.Count > 0)
                {
                    DataContext.SelectedTicket.MergeLines();
                    MenuItemSelectorViewModel.CloseView();
                    ActivateTicketView(null);
                }
                else
                {
                    CloseSelectedTicket();
                    ActivateTableView();
                }
            }
            else if (SelectedIndex == 3)
            {
                if (CanCloseSelectedTicket())
                {
                    CloseSelectedTicket();
                    ActivateTableView();
                }
            }
            else if (SelectedIndex == 2 && AppServices.MainDataContext.SelectedTicket != null)
            {
                SelectedIndex = 3;
            }
            else
            {
                TableScreenViewModel.CurrentPageNo = 0;
                TableScreenViewModel.HideFullScreenNumerator();
                MenuItemSelectorViewModel.CurrentScreenMenu = null;
                AppServices.LogoutUser();
                ActivateLoginView();
            }
        }

        void DepartmentSelectorViewModelDepartmentSelected(object sender, EventArgs e)
        {
            AppServices.MainDataContext.SelectedTableScreen = null;
            AppServices.MainDataContext.SelectedDepartment =
                DepartmentSelectorViewModel.SelectedDepartment;
            TicketEditorViewModel.ResetCache();
            ActivateTableView();
        }

        void TicketEditorViewModel_OnTicketTagEditorRequested(object sender, EventArgs e)
        {
            ActivateSelectedTicketItemEditorView(null, sender as TicketTagGroup);
        }

        void TicketEditorViewModel_OnTicketNoteEditorRequested(object sender, EventArgs e)
        {
            ActivateSelectedTicketItemEditorView(null, null);
        }

        void TicketEditorViewModel_OnSelectTableRequested(object sender, EventArgs e)
        {
            if (CanCloseSelectedTicket())
                ActivateTableView();
        }

        private void TicketEditorViewModel_OnCloseTicketRequested(object sender, EventArgs e)
        {
            CloseSelectedTicket();
            ActivateTableView();
        }

        private static void CloseSelectedTicket()
        {
            var result = DataContext.CloseSelectedTicket();
            if (!string.IsNullOrEmpty(result.ErrorMessage))
                ShowFeedback(result.ErrorMessage);
        }

        private void TicketEditorViewModel_OnAddMenuItemsRequested(object sender, EventArgs e)
        {
            ActivateMenuItemSelector();
        }

        void MenuItemSelectorViewModel_OnTicketItemSelected(TicketItemViewModel item)
        {
            ActivateSelectedTicketItemEditorView(item, null);
        }

        private void ActivateSelectedTicketItemEditorView(TicketItemViewModel item, TicketTagGroup selectedTicketTag)
        {
            SelectedIndex = 5;
            SelectedTicketItemEditorViewModel.Refresh(item, selectedTicketTag);
        }

        private void ActivateLoginView()
        {
            SelectedIndex = 0;
        }

        private void ActivateTableView()
        {
            SelectedIndex = 2;
            if (AppServices.MainDataContext.SelectedDepartment.TerminalTableScreenId > 0)
            {
                SelectedTicketViewIndex = 0;
                TableScreenViewModel.Refresh();
            }
            else
            {
                if (DataContext.SelectedTicket != null)
                {
                    if (DataContext.SelectedTicket.SelectedItems.Count > 0)
                    {
                        MoveSelectedItems(0);
                    }
                    ActivateTicketView(null);
                }
                else
                {
                    SelectedTicketViewIndex = 1;
                    TicketScreenViewModel.Refresh();
                }
            }
            LoggedInUserViewModel.Refresh();
        }

        private void ActivateTicketView(Table table)
        {
            if (table != null)
            {
                DataContext.UpdateSelectedTicket(table);
                if (table.TicketId == 0)
                {
                    ActivateMenuItemSelector();
                    return;
                }
            }
            LoggedInUserViewModel.Refresh();
            TicketEditorViewModel.Refresh();
            SelectedIndex = 3;
        }

        private void ActivateDepartmentSelector()
        {
            SelectedIndex = 1;
            DepartmentSelectorViewModel.Refresh();
        }

        private void ActivateMenuItemSelector()
        {
            SelectedIndex = 4;
            LoggedInUserViewModel.Refresh();
            MenuItemSelectorViewModel.Refresh();
        }

        internal static void ShowFeedback(string message)
        {
            var window = new FeedbackWindow { Message = { Text = message } };
            window.ShowDialog();
        }

        static bool CanCloseSelectedTicket()
        {
            var err = DataContext.SelectedTicket.GetPrintError();
            if (!string.IsNullOrEmpty(err))
            {
                ShowFeedback(err);
                return false;
            }
            return true;
        }

        private static void MoveSelectedItems(int tableId)
        {
            Debug.Assert(DataContext.SelectedTicket != null);
            Debug.Assert(DataContext.SelectedTicket.SelectedItems.Count > 0);
            DataContext.SelectedTicket.FixSelectedItems();
            var newTicketId = DataContext.MoveSelectedTicketItemsToNewTicket();
            DataContext.OpenTicket(newTicketId);
            if (tableId > 0)
                TicketViewModel.AssignLocationToSelectedTicket(tableId);
        }
    }
}
