namespace Samba.Services.Implementations.PrinterModule.Formatters
{
    internal class GenericFormatter : AbstractLineFormatter
    {
        public GenericFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        { }

        public override string GetFormattedLine()
        {
            return Tag.Tag + Line;
        }
    }
}