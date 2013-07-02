namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    internal class LeftAlignFormatter : AbstractLineFormatter
    {
        public LeftAlignFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        { }


        public override string GetFormattedLine()
        {
            return Line.PadRight(MaxWidth, ' ');
        }
    }    

}