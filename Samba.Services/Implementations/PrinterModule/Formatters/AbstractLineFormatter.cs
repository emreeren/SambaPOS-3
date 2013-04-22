namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    internal abstract class AbstractLineFormatter : ILineFormatter
    {
        public int FontWidth { get; set; }
        public int FontHeight { get; set; }
        protected string Line { get; set; }
        private int _maxWidth;
        protected int MaxWidth
        {
            get { return _maxWidth / (FontWidth + 1); }
            set { _maxWidth = value; }
        }

        public FormatTag Tag { get; set; }

        private static string RemoveTag(string line)
        {
            return line.Substring(line.IndexOf(">", System.StringComparison.Ordinal) + 1);
        }

        protected AbstractLineFormatter(string documentLine, int maxWidth)
        {
            Tag = new FormatTag(documentLine);
            MaxWidth = maxWidth;
            FontWidth = Tag.Width;
            FontHeight = Tag.Height;
            Line = RemoveTag(documentLine);
        }

        protected static string ExpandLabel(string label)
        {
            var result = "";
            for (var i = 0; i < label.Length - 1; i++)
            {
                result += label[i] + " ";
            }
            result += label[label.Length - 1];
            return " " + result.Trim() + " ";
        }

        public abstract string GetFormattedLine();

        public string GetFormattedLineWithoutTags()
        {
            var result = GetFormattedLine();
            if (!string.IsNullOrEmpty(Tag.TagName))
                result = result.Replace(Tag.Tag, "");
            return result;
        }
    }
}