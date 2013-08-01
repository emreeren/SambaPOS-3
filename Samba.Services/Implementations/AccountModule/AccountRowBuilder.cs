using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Localization;
using Samba.Persistance;
using Samba.Services.Common;

namespace Samba.Services.Implementations.AccountModule
{
    [Export]
    public class AccountRowBuilder
    {
        private readonly IAccountDao _accountDao;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public AccountRowBuilder(IAccountDao accountDao, ICacheService cacheService)
        {
            _accountDao = accountDao;
            _cacheService = cacheService;
        }

        public IEnumerable<AccountScreenRow> GetAccountScreenRows(AccountScreen accountScreen, WorkPeriod currentWorkPeriod)
        {
            var rows = new List<AccountScreenRow>();
            var detailedTemplateNames = accountScreen.AccountScreenValues.Where(x => x.DisplayDetails).Select(x => x.AccountTypeId);
            _accountDao.GetAccountBalances(detailedTemplateNames.ToList(), GetFilter(accountScreen, currentWorkPeriod)).ToList().ForEach(x => rows.Add(AccountScreenRow.Create(x, _cacheService.GetCurrencySymbol(x.Key.ForeignCurrencyId), GetGroupKey(accountScreen, x.Key.AccountTypeId))));

            var templateTotals = accountScreen.AccountScreenValues.Where(x => !x.DisplayDetails).Select(x => x.AccountTypeId);
            _accountDao.GetAccountTypeBalances(templateTotals.ToList(), GetFilter(accountScreen, currentWorkPeriod)).ToList().ForEach(x => rows.Add(AccountScreenRow.Create(x, GetGroupKey(accountScreen, x.Key.Id))));

            var hideIfZeroBalanceTypeIds =
                accountScreen.AccountScreenValues.Where(x => x.HideZeroBalanceAccounts).Select(x => x.AccountTypeId).ToList();

            var accounts = rows.Where(x => ShouldKeepAccount(x, hideIfZeroBalanceTypeIds))
                               .OrderBy(x => GetSortOrder(accountScreen.AccountScreenValues, x.AccountTypeId))
                               .ThenBy(x => x.Name)
                               .ToList();

            return accounts;
        }

        private static string GetGroupKey(AccountScreen accountScreen, int accountTypeId)
        {
            if (!accountScreen.DisplayAsTree) return null;
            return accountScreen.AccountScreenValues.Single(x => x.AccountTypeId == accountTypeId).AccountTypeName;
        }

        private static bool ShouldKeepAccount(AccountScreenRow accountRowData, ICollection<int> hideIfZeroBalanceTypeIds)
        {
            return !hideIfZeroBalanceTypeIds.Contains(accountRowData.AccountTypeId) ||
                   (hideIfZeroBalanceTypeIds.Contains(accountRowData.AccountTypeId) && accountRowData.Balance != 0);
        }

        private static int GetSortOrder(IEnumerable<AccountScreenValue> values, int accountTypeId)
        {
            return values.Single(x => x.AccountTypeId == accountTypeId).SortOrder;
        }

        private Expression<Func<AccountTransactionValue, bool>> GetFilter(AccountScreen selectedAccountScreen, WorkPeriod currentWorkPeriod)
        {
            if (selectedAccountScreen == null || selectedAccountScreen.Filter == 0) return null;
            //Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod
            if (selectedAccountScreen.Filter == 1)
            {
                var date = DateTime.Now.MonthStart();
                return x => x.Date >= date;
            }
            if (selectedAccountScreen.Filter == 2)
            {
                var date = DateTime.Now.StartOfWeek();
                return x => x.Date >= date;
            }
            if (selectedAccountScreen.Filter == 3)
            {
                var date = currentWorkPeriod.StartDate;
                return x => x.Date >= date;
            }
            return null;
        }
    }
}