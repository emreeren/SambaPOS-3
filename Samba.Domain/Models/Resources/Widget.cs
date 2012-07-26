using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class Widget : Entity
    {
        public int XLocation { get; set; }
        public int YLocation { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int CornerRadius { get; set; }
        public double Angle { get; set; }
        public string Properties { get; set; }
        public string CreatorName { get; set; }

        public void SaveSettings(object settingsObject)
        {
            Properties = settingsObject != null ? JsonHelper.Serialize(settingsObject) : "";
        }
    }

}
