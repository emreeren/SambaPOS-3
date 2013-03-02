﻿using System;
using System.Runtime.Serialization;
using Samba.Domain.Models.Menus;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class TaxValue
    {
        public TaxValue()
        {

        }

        public TaxValue(TaxTemplate taxTemplate)
        {
            TaxRate = taxTemplate.Rate;
            TaxTemplateId = taxTemplate.Id;
            TaxTemplateName = taxTemplate.Name;
            TaxTempleteAccountTransactionTypeId = taxTemplate.AccountTransactionType.Id;
        }

        [DataMember(Name = "TR")]
        public decimal TaxRate { get; set; }
        [DataMember(Name = "TN")]
        public string TaxTemplateName { get; set; }
        [DataMember(Name = "TT")]
        public int TaxTemplateId { get; set; }
        [DataMember(Name = "AT")]
        public int TaxTempleteAccountTransactionTypeId { get; set; }

        public decimal GetTax(bool taxIncluded, decimal price, decimal totalRate)
        {
            decimal result;
            if (taxIncluded && totalRate > 0)
            {
                result = decimal.Round((price * TaxRate) / (100 + totalRate), 2, MidpointRounding.AwayFromZero);
            }
            else if (TaxRate > 0) result = (price * TaxRate) / 100;
            else result = 0;
            return result;
        }

        public decimal GetTaxAmount(bool taxIncluded, decimal price, decimal totalRate, decimal plainSum, decimal preTaxServices)
        {
            if (preTaxServices != 0)
                price += (price * preTaxServices) / plainSum;
            var result = GetTax(taxIncluded, price, totalRate);
            return result;
        }
    }
}