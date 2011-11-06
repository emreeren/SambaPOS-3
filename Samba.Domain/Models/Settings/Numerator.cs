using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class Numerator : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
            Name = "Varsayılan Numeratör";
            Number = 0;
        }
    }
}
