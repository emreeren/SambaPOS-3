namespace Samba.Presentation.Services.Implementations.PrinterModule.ValueChangers
{
    class TaxValue
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal OrderAmount { get; set; }
    }

    class TaxValueChanger : AbstractValueChanger<TaxValue>
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
