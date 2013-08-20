namespace Samba.Services.Common
{
    public interface IProgramSettings
    {
        string QuantitySeparators { get; set; }
        string WeightBarcodePrefix { get; set; }
        int WeightBarcodeItemLength { get; set; }
        string WeightBarcodeItemFormat { get; set; }
        int WeightBarcodeQuantityLength { get; set; }
        decimal AutoRoundDiscount { get; set; }
        string PaymentScreenValues { get; set; }
        string UserInfo { get; set; }
    }
}