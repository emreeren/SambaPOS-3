namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    class CenterAlignFormatter : AbstractLineFormatter
    {
        public CenterAlignFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            return Line.PadLeft(((MaxWidth + Line.Length) / 2), ' ').PadRight(MaxWidth);
        }
    }
}
