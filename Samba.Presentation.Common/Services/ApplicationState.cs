using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.Services
{
    [Export(typeof(IApplicationState))]
    [Export(typeof(IApplicationStateSetter))]
    public class ApplicationState : AbstractService, IApplicationState, IApplicationStateSetter
    {
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public ApplicationState(IDepartmentService departmentService, ISettingService settingService,
            ICacheService cacheService)
        {
            _departmentService = departmentService;
            _settingService = settingService;
            _cacheService = cacheService;
            CurrentTicketType = TicketType.Default;
        }

        public AppScreens ActiveAppScreen { get; private set; }
        public CurrentDepartmentData CurrentDepartment { get; private set; }
        public TicketType CurrentTicketType { get; set; }
        public ResourceScreen SelectedResourceScreen { get; private set; }
        public ResourceScreen ActiveResourceScreen { get; set; }

        private Terminal _terminal;

        private bool _isLocked;
        public bool IsLocked
        {
            get { return _isLocked; }
        }

        public Terminal CurrentTerminal { get { return _terminal ?? (_terminal = GetCurrentTerminal()); } set { _terminal = value; } }

        private User _currentLoggedInUser;

        public User CurrentLoggedInUser
        {
            get { return _currentLoggedInUser ?? User.Nobody; }
            private set { _currentLoggedInUser = value; }
        }

        private IEnumerable<WorkPeriod> _lastTwoWorkPeriods;
        public IEnumerable<WorkPeriod> LastTwoWorkPeriods
        {
            get { return _lastTwoWorkPeriods ?? (_lastTwoWorkPeriods = Dao.Last<WorkPeriod>(2)); }
        }

        public WorkPeriod CurrentWorkPeriod
        {
            get { return LastTwoWorkPeriods.LastOrDefault(); }
        }

        public WorkPeriod PreviousWorkPeriod
        {
            get { return LastTwoWorkPeriods.Count() > 1 ? LastTwoWorkPeriods.FirstOrDefault() : null; }
        }

        public bool IsCurrentWorkPeriodOpen
        {
            get
            {
                return CurrentWorkPeriod != null && CurrentWorkPeriod.StartDate == CurrentWorkPeriod.EndDate;
            }
        }

        public void SetCurrentLoggedInUser(User user)
        {
            CurrentLoggedInUser = user;
        }

        public void SetCurrentDepartment(Department department)
        {
            if (CurrentDepartment == null || department != CurrentDepartment.Model)
            {
                CurrentDepartment = new CurrentDepartmentData { Model = department };
                CurrentDepartment.Model.PublishEvent(EventTopicNames.SelectedDepartmentChanged);
            }
        }

        public void SetCurrentDepartment(int departmentId)
        {
            SetCurrentDepartment(_departmentService.GetDepartment(departmentId));
        }

        public void SetCurrentApplicationScreen(AppScreens appScreen)
        {
            ActiveAppScreen = appScreen;
        }

        public ResourceScreen SetSelectedResourceScreen(ResourceScreen resourceScreen)
        {
            if (IsLocked && ActiveResourceScreen == null) ActiveResourceScreen = SelectedResourceScreen;
            else if (!IsLocked && ActiveResourceScreen != null)
            {
                resourceScreen = ActiveResourceScreen;
                ActiveResourceScreen = null;
            }
            SelectedResourceScreen = resourceScreen;
            return resourceScreen;
        }

        public void SetApplicationLocked(bool isLocked)
        {
            _isLocked = isLocked;
            (this as IApplicationState).PublishEvent(EventTopicNames.ApplicationLockStateChanged);
        }

        public void SetNumberpadValue(string value)
        {
            _settingService.ReadLocalSetting("NUMBERPAD").StringValue = value;
        }

        public IEnumerable<PaidItem> LastPaidItems { get; private set; }

        public void SetLastPaidItems(IEnumerable<PaidItem> paidItems)
        {
            LastPaidItems = paidItems;
        }

        public void SetCurrentTicketType(TicketType ticketType)
        {
            CurrentTicketType = ticketType??TicketType.Default;
        }

        public string NumberPadValue
        {
            get { return _settingService.ReadLocalSetting("NUMBERPAD").StringValue; }
        }

        private Terminal GetCurrentTerminal()
        {
            if (!string.IsNullOrEmpty(LocalSettings.TerminalName))
            {
                var terminal = _settingService.GetTerminalByName(LocalSettings.TerminalName);
                if (terminal != null) return terminal;
            }
            var dterminal = _settingService.GetDefaultTerminal();
            return dterminal ?? Terminal.DefaultTerminal;
        }

        public void ResetWorkPeriods()
        {
            _lastTwoWorkPeriods = null;
        }

        public override void Reset()
        {
            _cacheService.ResetCache();
            _lastTwoWorkPeriods = null;
            _terminal = null;
        }

        public ProductTimer GetProductTimer(int menuItemId)
        {
            return _cacheService.GetProductTimer(CurrentTicketType.Id,
                                                 CurrentTerminal.Id,
                                                 CurrentDepartment.Id,
                                                 CurrentLoggedInUser.UserRole.Id,
                                                 menuItemId);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroups(params int[] menuItemIds)
        {
            return _cacheService.GetOrderTagGroups(CurrentTicketType.Id,
                                                   CurrentTerminal.Id,
                                                   CurrentDepartment.Id,
                                                   CurrentLoggedInUser.UserRole.Id,
                                                   menuItemIds);
        }

        public IEnumerable<OrderStateGroup> GetOrderStateGroups(params int[] menuItemIds)
        {
            return _cacheService.GetOrderStateGroups(CurrentTicketType.Id,
                                                     CurrentTerminal.Id,
                                                     CurrentDepartment.Id,
                                                     CurrentLoggedInUser.UserRole.Id,
                                                     menuItemIds);
        }

        public IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId)
        {
            return _cacheService.GetAccountTransactionDocumentTypes(accountTypeId,
                                                                    CurrentTerminal.Id,
                                                                    CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList)
        {
            return _cacheService.GetBatchDocumentTypes(accountTypeNamesList, CurrentTerminal.Id,
                                                       CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<PaymentType> GetUnderTicketPaymentTypes()
        {
            return _cacheService.GetUnderTicketPaymentTypes(CurrentTicketType.Id,
                                                            CurrentTerminal.Id,
                                                            CurrentDepartment.Id,
                                                            CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<PaymentType> GetPaymentScreenPaymentTypes()
        {
            return _cacheService.GetPaymentScreenPaymentTypes(CurrentTicketType.Id,
                                                            CurrentTerminal.Id,
                                                            CurrentDepartment.Id,
                                                            CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<ChangePaymentType> GetChangePaymentTypes()
        {
            return _cacheService.GetChangePaymentTypes(CurrentTicketType.Id,
                                                       CurrentTerminal.Id,
                                                       CurrentDepartment.Id,
                                                       CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<TicketTagGroup> GetTicketTagGroups()
        {
            return _cacheService.GetTicketTagGroups(CurrentTicketType.Id,
                                                    CurrentTerminal.Id,
                                                    CurrentDepartment.Id,
                                                    CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<AutomationCommandData> GetAutomationCommands()
        {
            return _cacheService.GetAutomationCommands(CurrentTicketType.Id,
                                                       CurrentTerminal.Id,
                                                       CurrentDepartment != null ? CurrentDepartment.Id : -1,
                                                       CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<CalculationSelector> GetCalculationSelectors()
        {
            return _cacheService.GetCalculationSelectors(CurrentTicketType.Id,
                                                         CurrentTerminal.Id,
                                                         CurrentDepartment.Id,
                                                         CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<ResourceScreen> GetResourceScreens()
        {
            return _cacheService.GetResourceScreens(CurrentTerminal.Id,
                                                    CurrentDepartment.Id,
                                                    CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<ResourceScreen> GetTicketResourceScreens()
        {
            return _cacheService.GetTicketResourceScreens(CurrentTicketType != null ? CurrentTicketType.Id : 0,
                                                       CurrentTerminal.Id,
                                                       CurrentDepartment.Id,
                                                       CurrentLoggedInUser.UserRole.Id);
        }

        public void ResetState()
        {
            _departmentService.Reset();

            var did = CurrentDepartment.Id;
            CurrentDepartment = null;
            SetCurrentDepartment(did);
        }
    }
}
