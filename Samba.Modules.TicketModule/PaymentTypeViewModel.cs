﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class PaymentTypeViewModel : EntityViewModelBaseWithMap<PaymentType, PaymentTypeMap, PaymentTypeMapViewModel>
    {
        [ImportingConstructor]
        public PaymentTypeViewModel()
        {
        }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes
        {
            get
            {
                return _accountTransactionTypes ?? (_accountTransactionTypes =
                    Workspace.All<AccountTransactionType>().ToList());
            }
        }

        private IEnumerable<Account> _accounts;
        public IEnumerable<Account> Accounts
        {
            get
            {
                return _accounts ?? (_accounts =
                    AccountTransactionType != null
                    ? Workspace.All<Account>(x => x.AccountTypeId == AccountTransactionType.TargetAccountTypeId).ToList()
                    : new List<Account>());
            }
        }

        public AccountTransactionType AccountTransactionType
        {
            get { return Model.AccountTransactionType; }
            set
            {
                Model.AccountTransactionType = value;
                Account = null;
                _accounts = null;
                RaisePropertyChanged(() => Accounts);
            }
        }

        public Account Account
        {
            get { return Model.Account; }
            set
            {
                Model.Account = value;
                RaisePropertyChanged(() => Account);
            }
        }

        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }

        public override Type GetViewType()
        {
            return typeof(PaymentTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.PaymentType;
        }

        protected override AbstractValidator<PaymentType> GetValidator()
        {
            return new PaymentTypeValidator();
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<PaymentTypeMap, PaymentTypeMapViewModel>(Model.PaymentTypeMaps, Workspace);
        }
    }

    internal class PaymentTypeValidator : EntityValidator<PaymentType>
    {
        public PaymentTypeValidator()
        {
            RuleFor(x => x.AccountTransactionType).NotNull();
            //RuleFor(x => x.Account).NotNull();
        }
    }
}
