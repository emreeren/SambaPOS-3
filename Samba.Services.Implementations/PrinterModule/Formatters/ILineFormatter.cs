namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    public interface ILineFormatter
    {
        int FontWidth { get; set; }
        int FontHeight { get; set; }
        FormatTag Tag { get; set; }
        string GetFormattedLine();
        string GetFormattedLineWithoutTags();
    }
}
