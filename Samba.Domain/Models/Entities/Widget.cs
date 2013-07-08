using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;

namespace Samba.Domain.Models.Entities
{
    public class Widget : ValueClass
    {
        public string Name { get; set; }
        public int EntityScreenId { get; set; }
        public int XLocation { get; set; }
        public int YLocation { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int CornerRadius { get; set; }
        public double Angle { get; set; }
        public double Scale { get; set; }
        public string Properties { get; set; }
        public string CreatorName { get; set; }
        public bool AutoRefresh { get; set; }
        public int AutoRefreshInterval { get; set; }

        public void SaveSettings(object settingsObject)
        {
            Properties = settingsObject != null ? JsonHelper.Serialize(settingsObject) : "";
        }
    }

}
