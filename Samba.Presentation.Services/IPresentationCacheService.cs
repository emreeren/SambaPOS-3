using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Services.Common;

namespace Samba.Presentation.Services
{
    public interface IPresentationCacheService
    {
        ProductTimer GetProductTimer(int menuItemId);
        IEnumerable<OrderTagGroup> GetOrderTagGroups(params int[] menuItemIds);
        IEnumerable<OrderStateGroup> GetOrderStateGroups(params int[] menuItemIds);
        IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId);
        IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList);
        IEnumerable<PaymentType> GetUnderTicketPaymentTypes();
        IEnumerable<PaymentType> GetPaymentScreenPaymentTypes();
        IEnumerable<ChangePaymentType> GetChangePaymentTypes();
        IEnumerable<TicketTagGroup> GetTicketTagGroups();
        IEnumerable<AutomationCommandData> GetAutomationCommands();
        IEnumerable<CalculationSelector> GetCalculationSelectors();
        IEnumerable<ResourceScreen> GetResourceScreens();
        IEnumerable<ResourceScreen> GetTicketResourceScreens();

    }
}
