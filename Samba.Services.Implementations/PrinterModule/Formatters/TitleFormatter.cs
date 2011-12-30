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
            if (expandLabel) label = ExpandLabel(label);
            string text = label.PadLeft((((MaxWidth) + label.Length) / 2), fillChar);
            return text + "".PadLeft(MaxWidth - text.Length, fillChar);
        }
    }
}