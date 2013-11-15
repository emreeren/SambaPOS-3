using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Persistance;
using Samba.Persistance.Data;

namespace Samba.Services.Tests
{
    [TestFixture]
    class AccountTests
    {
        protected IAccountService AccountService { get; set; }
        protected IAccountDao AccountDao { get; set; }
        protected IWorkspace Workspace { get; set; }
        public ICacheService CacheService { get; set; }


        [SetUp]
        public void Setup()
        {
            MefBootstrapper.ComposeParts();
            AccountService = MefBootstrapper.Resolve<IAccountService>();
            CacheService = MefBootstrapper.Resolve<ICacheService>();
            AccountDao = MefBootstrapper.Resolve<IAccountDao>();
            Workspace = PrepareWorkspace("sd1.txt");
        }

        [Test]
        public void CanCreateCashAccount()
        {
            var accountType = AccountTypeBuilder.Create("Payment Accounts").Build();
            Workspace.Add(accountType);
            var account = AccountBuilder.Create("Cash").WithAccountType(accountType).Build();
            Workspace.Add(account);
            Workspace.CommitChanges();
            var balance = AccountService.GetAccountBalance(account.Id);
            Assert.AreEqual(0, balance);
        }

        [Test]
        public void CanDebitCashAccount()
        {
            var testContext = new AccountTestContext();
            testContext.Create(Workspace, AccountService);
            testContext.MakeSale(100);
            Workspace.CommitChanges();
            var balance = AccountService.GetAccountBalance(testContext.CashAccount.Id);
            Assert.AreEqual(100, balance);
        }

        [Test]
        public void CanDebitCashAccountMultipleTimes()
        {
            var testContext = new AccountTestContext();
            testContext.Create(Workspace, AccountService);
            testContext.MakeSale(100);
            testContext.MakeSale(200);
            var balance = AccountService.GetAccountBalance(testContext.CashAccount.Id);
            var saleBalance = AccountService.GetAccountBalance(testContext.SaleAccount.Id);
            Assert.AreEqual(0, balance + saleBalance);
        }

        [Test]
        public void CanCreditCashAccountMultipleTimes()
        {
            var testContext = new AccountTestContext();
            testContext.Create(Workspace, AccountService);
            testContext.MakeSale(100);
            testContext.MakeSale(200);
            testContext.MakeRefund(50);
            var balance = AccountService.GetAccountBalance(testContext.CashAccount.Id);
            Assert.AreEqual(250, balance);
        }

        [Test]
        public void CanCalculateCashAccountMultipleTimes()
        {
            var testContext = new AccountTestContext();
            testContext.Create(Workspace, AccountService);
            testContext.MakeSale(100);
            testContext.MakeRefund(50);
            testContext.MakeSale(100);
            testContext.MakeRefund(10);
            testContext.MakeRefund(40);
            var balance = AccountService.GetAccountBalance(testContext.CashAccount.Id);
            Assert.AreEqual(100, balance);
        }

        [Test]
        public void CanCalculateUsdCashAccountMultipleTimes()
        {
            var testContext = new AccountTestContext();
            testContext.Create(Workspace, AccountService);
            testContext.MakeUsdSale(100);
            var usdBalance = AccountDao.GetAccountExchangeBalance(testContext.UsdCashAccount.Id);
            Assert.AreEqual(100, usdBalance);
            var saleBalance = AccountDao.GetAccountExchangeBalance(testContext.SaleAccount.Id);
            Assert.AreEqual(-200, saleBalance);
        }

        [Test]
        public void CanCalculateRefundUsdCashAccountMultipleTimes()
        {
            var testContext = new AccountTestContext();
            testContext.Create(Workspace, AccountService);
            testContext.MakeUsdSale(100);
            testContext.MakeUsdRefund(50);
            var usdBalance = AccountDao.GetAccountExchangeBalance(testContext.UsdCashAccount.Id);
            Assert.AreEqual(50, usdBalance);
            var saleBalance = AccountDao.GetAccountExchangeBalance(testContext.SaleAccount.Id);
            Assert.AreEqual(-100, saleBalance);
        }

        private IWorkspace PrepareWorkspace(string fileName)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            var lp = new Uri(ass.CodeBase);
            string pth = Path.GetDirectoryName(lp.LocalPath);
            pth = Path.Combine(pth, "..\\..\\..\\Samba.Presentation");
            LocalSettings.AppPath = pth;
            LocalSettings.CurrentLanguage = "en";
            var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\tests";
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            var filePath = string.Format("{0}\\{1}", dataFolder, fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
            WorkspaceFactory.UpdateConnection(filePath);
            var workspace = WorkspaceFactory.Create();
            workspace.CommitChanges();
            return workspace;
        }
    }

    internal class AccountTestContext
    {
        private IAccountService _accountService;
        public void Create(IWorkspace workspace, IAccountService accountService)
        {
            _accountService = accountService;
            Workspace = workspace;
            Usd = CreateForeignCurrency("USD", "$", 2);
            PaymentAccountType = CreateAccountType("Payment Accounts", 1);
            SaleAccountType = CreateAccountType("Sale Accounts", 2);
            SaleAccount = CreateAccount(SaleAccountType, "Sales", 91);
            CashAccount = CreateAccount(PaymentAccountType, "Cash", 92);
            UsdCashAccount = CreateAccount(PaymentAccountType, "UsdCash", 93, Usd);
            SaleTransactionType = CreateTransactionType(SaleAccount, CashAccount, ForeignCurrency.Default);
            RefundTransactionType = CreateTransactionType(CashAccount, SaleAccount, ForeignCurrency.Default);
            SaleTransactionDocument = CreateTransactionDocument(SaleAccountType, SaleTransactionType);
            RefundTransactionDocument = CreateTransactionDocument(PaymentAccountType, RefundTransactionType);
            UsdSaleTransactionType = CreateTransactionType(SaleAccount, UsdCashAccount, Usd);
            UsdRefundTransactionType = CreateTransactionType(UsdCashAccount, SaleAccount, Usd);
            UsdSaleTransactionDocument = CreateTransactionDocument(SaleAccountType, UsdSaleTransactionType);
            UsdRefundTransactionDocument = CreateTransactionDocument(PaymentAccountType, UsdRefundTransactionType);
        }

        public AccountTransactionDocumentType UsdRefundTransactionDocument { get; set; }
        public AccountTransactionDocumentType UsdSaleTransactionDocument { get; set; }
        public AccountTransactionType UsdRefundTransactionType { get; set; }
        public AccountTransactionType UsdSaleTransactionType { get; set; }
        public AccountTransactionType RefundTransactionType { get; set; }
        public AccountTransactionDocumentType RefundTransactionDocument { get; set; }
        public AccountTransactionDocumentType SaleTransactionDocument { get; set; }
        public AccountTransactionType SaleTransactionType { get; set; }
        public Account CashAccount { get; set; }
        public Account UsdCashAccount { get; set; }
        public Account SaleAccount { get; set; }
        public AccountType SaleAccountType { get; set; }
        public AccountType PaymentAccountType { get; set; }
        public ForeignCurrency Usd { get; set; }
        public IWorkspace Workspace { get; set; }

        private ForeignCurrency CreateForeignCurrency(string name, string symbol, decimal exchangeRate)
        {
            var result = new ForeignCurrency { CurrencySymbol = symbol + "{0}", ExchangeRate = exchangeRate, Name = name };
            Workspace.Add(result);
            return result;
        }

        private AccountTransactionDocumentType CreateTransactionDocument(AccountType masterAccountType, params AccountTransactionType[] transactionTypes)
        {
            var result = new AccountTransactionDocumentType();
            foreach (var accountTransactionType in transactionTypes)
            {
                result.TransactionTypes.Add(accountTransactionType);
            }
            result.MasterAccountTypeId = masterAccountType.Id;
            Workspace.Add(result);
            Workspace.CommitChanges();
            return result;
        }

        private AccountTransactionType CreateTransactionType(Account sourceAccount, Account targetAccount, ForeignCurrency foreignCurrency)
        {
            var result = new AccountTransactionType
             {
                 SourceAccountTypeId = sourceAccount.AccountTypeId,
                 TargetAccountTypeId = targetAccount.AccountTypeId,
                 DefaultSourceAccountId = sourceAccount.Id,
                 DefaultTargetAccountId = targetAccount.Id,
                 ForeignCurrencyId = foreignCurrency.Id
             };

            Workspace.Add(result);
            Workspace.CommitChanges();
            return result;
        }

        private Account CreateAccount(AccountType accountType, string accountName, int accountId, ForeignCurrency currency = null)
        {
            var result = AccountBuilder.Create(accountName)
                .WithId(accountId)
                .WithAccountType(accountType)
                .WithForeignCurrency(currency)
                .Build();
            Workspace.Add(result);
            Workspace.CommitChanges();
            return result;
        }

        private AccountType CreateAccountType(string accountName, int accountId)
        {
            var result = AccountTypeBuilder.Create(accountName).WithId(accountId).Build();
            Workspace.Add(result);
            return result;
        }

        private void CreateTransaction(AccountTransactionDocumentType accountTransactionDocumentType, Account masterAccount, decimal amount, params Account[] accounts)
        {
            _accountService.CreateTransactionDocument(masterAccount, accountTransactionDocumentType, "", amount, accounts);
            Workspace.CommitChanges();
        }

        public void MakeSale(decimal amount)
        {
            CreateTransaction(SaleTransactionDocument, SaleAccount, amount, CashAccount);
        }

        public void MakeRefund(decimal amount)
        {
            CreateTransaction(RefundTransactionDocument, CashAccount, amount, SaleAccount);
        }

        public void MakeUsdSale(decimal amount)
        {
            CreateTransaction(UsdSaleTransactionDocument, SaleAccount, amount, UsdCashAccount);
        }

        public void MakeUsdRefund(decimal amount)
        {
            CreateTransaction(UsdRefundTransactionDocument, UsdCashAccount, amount, SaleAccount);
        }
    }

    internal class AccountBuilder
    {
        private readonly string _accountName;
        private int _accountId;
        private AccountType _accountType;
        private ForeignCurrency _foreignCurrency;

        private AccountBuilder(string accountName)
        {
            _accountName = accountName;
            _foreignCurrency = ForeignCurrency.Default;
        }

        public static AccountBuilder Create(string accountName)
        {
            return new AccountBuilder(accountName);
        }

        public AccountBuilder WithId(int accountId)
        {
            _accountId = accountId;
            return this;
        }

        public AccountBuilder WithAccountType(AccountType accountType)
        {
            _accountType = accountType;
            return this;
        }

        public AccountBuilder WithForeignCurrency(ForeignCurrency foreignCurrency)
        {
            if (foreignCurrency != null)
                _foreignCurrency = foreignCurrency;
            return this;
        }

        public Account Build()
        {
            return new Account
                   {
                       AccountTypeId = _accountType.Id,
                       Id = _accountId,
                       Name = _accountName,
                       ForeignCurrencyId = _foreignCurrency.Id
                   };
        }
    }

    internal class AccountTypeBuilder
    {
        private readonly string _accountTypeName;
        private int _accountId;

        private AccountTypeBuilder(string accountTypeName)
        {
            _accountTypeName = accountTypeName;
        }

        public static AccountTypeBuilder Create(string accountTypeName)
        {
            return new AccountTypeBuilder(accountTypeName);
        }

        public AccountTypeBuilder WithId(int accountId)
        {
            _accountId = accountId;
            return this;
        }

        public AccountType Build()
        {
            return new AccountType { Name = _accountTypeName, Id = _accountId };
        }
    }
}
