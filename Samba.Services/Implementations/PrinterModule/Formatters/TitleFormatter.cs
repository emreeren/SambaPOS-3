namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    internal class TitleFormatter : AbstractLineFormatter
    {
        public TitleFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {

        }

        public override string GetFormattedLine()
        {
            return PrintCenteredLabel(Line, true);
        }

        private string PrintCenteredLabel(string label, bool expandLabel, char fillChar = '░')
        {
            if (string.IsNullOrEmpty(label)) return "".PadLeft(MaxWidth, fillChar); 
            if (expandLabel) label = ExpandLabel(label);
            var leftPad = ((MaxWidth) + label.Length);
            if (leftPad % 2 == 1) leftPad++;
            string text = label.PadLeft(leftPad/2, fillChar);
            return text + "".PadLeft(MaxWidth - text.Length, fillChar);
        }
    }
}