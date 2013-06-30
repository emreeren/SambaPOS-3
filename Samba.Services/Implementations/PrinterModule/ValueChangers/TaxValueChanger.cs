using System.ComponentModel.Composition;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TaxValue
    {
        public bool TaxIncluded { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal OrderAmount { get { return OrderTotal - (TaxIncluded ? TaxAmount : 0); } }
        public decimal TotalAmount { get { return OrderTotal; } }
    }

    [Export]
    public class TaxValueChanger : AbstractValueChanger<TaxValue>
    {
        public override string GetTargetTag()
        {
            return "TAXES";
        }

        protected override string GetModelName(TaxValue model)
        {
            return model.Name;
        }
    }
}
