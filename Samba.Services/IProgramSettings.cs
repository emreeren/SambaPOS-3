namespace Samba.Services
{
    public interface IProgramSettings
    {
        string WeightBarcodePrefix { get; set; }
        int WeightBarcodeItemLength { get; set; }
        string WeightBarcodeItemFormat { get; set; }
        int WeightBarcodeQuantityLength { get; set; }
        decimal AutoRoundDiscount { get; set; }
    }
}