namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    class RightAlignFormatter : AbstractLineFormatter
    {
        public RightAlignFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            return Line.PadLeft(MaxWidth, ' ');
        }
    }
}
