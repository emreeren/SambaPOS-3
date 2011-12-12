using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Locations
{
    public class LocationScreen : IEntity, IOrderable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public int DisplayMode { get; set; }
        public string BackgroundColor { get; set; }
        public string BackgroundImage { get; set; }
        public string LocationEmptyColor { get; set; }
        public string LocationFullColor { get; set; }
        public string LocationLockedColor { get; set; }
        public int PageCount { get; set; }
        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public int NumeratorHeight { get; set; }
        public string AlphaButtonValues { get; set; }

        private IList<Location> _locations;
        public virtual IList<Location> Locations
        {
            get { return _locations; }
            set { _locations = value; }
        }

        public string UserString
        {
            get { return Name; }
        }

        public bool IsBackgroundImageVisible { get { return !string.IsNullOrEmpty(BackgroundImage); } }

        public LocationScreen()
        {
            _locations = new List<Location>();
            LocationEmptyColor = "WhiteSmoke";
            LocationFullColor = "Orange";
            LocationLockedColor = "Brown";
            BackgroundColor = "Transparent";
            PageCount = 1;
            ButtonHeight = 0;
        }

        public int ItemCountPerPage
        {
            get
            {
                var itemCount = Locations.Count / PageCount;
                if (Locations.Count % PageCount > 0) itemCount++;
                return itemCount;
            }
        }

        public void AddScreenItem(Location choosenValue)
        {
            if (!Locations.Contains(choosenValue))
                Locations.Add(choosenValue);
        }
    }
}
