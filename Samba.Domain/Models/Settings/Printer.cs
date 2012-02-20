using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class Printer : Entity
    {
        public byte[] LastUpdateTime { get; set; }
        public string ShareName { get; set; }
        public int PrinterType { get; set; }
        public int CodePage { get; set; }
        public int CharsPerLine { get; set; }
        public int PageHeight { get; set; }

        public Printer()
        {
            CharsPerLine = 42;
            CodePage = 857;
        }
    }
}
