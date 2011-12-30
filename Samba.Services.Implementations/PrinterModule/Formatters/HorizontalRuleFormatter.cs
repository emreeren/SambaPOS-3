namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    internal class HorizontalRuleFormatter : AbstractLineFormatter
    {
        public HorizontalRuleFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            var result = Line.Trim();
            if (result.Length > 0)
                return "".PadLeft(MaxWidth, result[0]);
            return result;
        }
    }
}