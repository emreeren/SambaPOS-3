using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class Numerator : Entity
    {
        public byte[] LastUpdateTime { get; set; }
        public int Number { get; set; }
        public string NumberFormat { get; set; }

        public string GetNumber()
        {
            return Number.ToString(NumberFormat);
        }

        public Numerator()
        {
            NumberFormat = "#";
            Number = 0;
        }
    }
}
