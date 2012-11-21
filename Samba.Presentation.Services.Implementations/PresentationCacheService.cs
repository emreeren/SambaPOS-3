using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Implementations
{
    [Export(typeof(IPresentationCacheService))]
    class PresentationCacheService : AbstractService, IPresentationCacheService
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PresentationCacheService(IApplicationState applicationState, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
        }

        public ProductTimer GetProductTimer(int menuItemId)
        {
            return _cacheService.GetProductTimer(_applicationState.CurrentTicketType.Id,
                                                 _applicationState.CurrentTerminal.Id,
                                                 _applicationState.CurrentDepartment.Id,
                                                 _applicationState.CurrentLoggedInUser.UserRole.Id,
                                                 menuItemId);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroups(params int[] menuItemIds)
        {
            return _cacheService.GetOrderTagGroups(_applicationState.CurrentTicketType.Id,
                                                   _applicationState.CurrentTerminal.Id,
                                                   _applicationState.CurrentDepartment.Id,
                                                   _applicationState.CurrentLoggedInUser.UserRole.Id,
                                                   menuItemIds);
        }

        public IEnumerable<OrderStateGroup> GetOrderStateGroups(params int[] menuItemIds)
        {
            return _cacheService.GetOrderStateGroups(_applicationState.CurrentTicketType.Id,
                                                     _applicationState.CurrentTerminal.Id,
                                                     _applicationState.CurrentDepartment.Id,
                                                     _applicationState.CurrentLoggedInUser.UserRole.Id,
                                                     menuItemIds);
        }

        public IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId)
        {
            return _cacheService.GetAccountTransactionDocumentTypes(accountTypeId,
                                                                    _applicationState.CurrentTerminal.Id,
                                                                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList)
        {
            return _cacheService.GetBatchDocumentTypes(accountTypeNamesList, _applicationState.CurrentTerminal.Id,
                                                       _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<PaymentType> GetUnderTicketPaymentTypes()
        {
            return _cacheService.GetUnderTicketPaymentTypes(_applicationState.CurrentTicketType.Id,
                                                            _applicationState.CurrentTerminal.Id,
                                                            _applicationState.CurrentDepartment.Id,
                                                            _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<PaymentType> GetPaymentScreenPaymentTypes()
        {
            return _cacheService.GetPaymentScreenPaymentTypes(_applicationState.CurrentTicketType.Id,
                                                            _applicationState.CurrentTerminal.Id,
                                                            _applicationState.CurrentDepartment.Id,
                                                            _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<ChangePaymentType> GetChangePaymentTypes()
        {
            return _cacheService.GetChangePaymentTypes(_applicationState.CurrentTicketType.Id,
                                                       _applicationState.CurrentTerminal.Id,
                                                       _applicationState.CurrentDepartment.Id,
                                                       _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<TicketTagGroup> GetTicketTagGroups()
        {
            return _cacheService.GetTicketTagGroups(_applicationState.CurrentTicketType.Id,
                                                    _applicationState.CurrentTerminal.Id,
                                                    _applicationState.CurrentDepartment.Id,
                                                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<AutomationCommandData> GetAutomationCommands()
        {
            var currentDepartmentId = _applicationState.CurrentDepartment != null
                                          ? _applicationState.CurrentDepartment.Id
                                          : -1;
            return _cacheService.GetAutomationCommands(_applicationState.CurrentTicketType.Id,
                                                       _applicationState.CurrentTerminal.Id,
                                                       currentDepartmentId,
                                                       _applicationState.CurrentLoggedInUser.UserRole.Id);
        }
        
        public IEnumerable<CalculationSelector> GetCalculationSelectors()
        {
            return _cacheService.GetCalculationSelectors(_applicationState.CurrentTicketType.Id,
                                                         _applicationState.CurrentTerminal.Id,
                                                         _applicationState.CurrentDepartment.Id,
                                                         _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<ResourceScreen> GetResourceScreens()
        {
            return _cacheService.GetResourceScreens(_applicationState.CurrentTerminal.Id,
                                                    _applicationState.CurrentDepartment.Id,
                                                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<ResourceScreen> GetTicketResourceScreens()
        {
            return
                _cacheService.GetTicketResourceScreens(
                    _applicationState.CurrentTicketType != null ? _applicationState.CurrentTicketType.Id : 0,
                    _applicationState.CurrentTerminal.Id,
                    _applicationState.CurrentDepartment.Id,
                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public override void Reset()
        {
            _cacheService.ResetCache();
        }
    }
}
