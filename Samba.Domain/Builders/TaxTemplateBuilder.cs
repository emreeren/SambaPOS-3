using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;

namespace Samba.Domain.Builders
{
    public class TaxTemplateBuilder : ILinkableToAccountTransactionTypeBuilder<TaxTemplateBuilder>
    {
        private readonly string _taxName;
        private decimal _rate;
        private AccountTransactionType _accountTransactionType;
        private int _rounding;
        private readonly IList<TaxTemplateMap> _taxTemplateMaps;

        private TaxTemplateBuilder(string taxName)
        {
            _taxName = taxName;
            _taxTemplateMaps = new List<TaxTemplateMap>();
        }

        public static TaxTemplateBuilder Create(string taxName)
        {
            return new TaxTemplateBuilder(taxName);
        }

        public TaxTemplateBuilder WithRate(decimal rate)
        {
            _rate = rate;
            return this;
        }

        public TaxTemplateBuilder WithAccountTransactionType(AccountTransactionType accountTransactionType)
        {
            _accountTransactionType = accountTransactionType;
            return this;
        }

        public TaxTemplateBuilder WithRounding(int rounding)
        {
            _rounding = rounding;
            return this;
        }

        public TaxTemplate Build()
        {
            var result = new TaxTemplate
                             {
                                 AccountTransactionType = _accountTransactionType,
                                 Name = _taxName,
                                 Rate = _rate,
                                 Rounding = _rounding
                             };

            foreach (var taxTemplateMap in _taxTemplateMaps)
            {
                result.TaxTemplateMaps.Add(taxTemplateMap);
            }
            return result;
        }

        public void Link(AccountTransactionTypeBuilder accountTransactionTypeBuilder)
        {
            WithAccountTransactionType(accountTransactionTypeBuilder.Build());
        }

        public AccountTransactionTypeBuilderFor<TaxTemplateBuilder> CreateAccountTransactionType()
        {
            return new AccountTransactionTypeBuilderFor<TaxTemplateBuilder>(this);
        }

        public TaxTemplateBuilder AddDefaultTaxTemplateMap()
        {
            _taxTemplateMaps.Add(new TaxTemplateMap());
            return this;
        }

        public TaxTemplateBuilder AddTaxTemplateMap(TaxTemplateMap taxTemplateMap)
        {
            _taxTemplateMaps.Add(taxTemplateMap);
            return this;
        }
    }
}