using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Entities
{
    public class EntityScreen : EntityClass, IOrderable
    {
        //todo direk referansları bul ve departman ayarlarını da içerecek şekilde state'den oku.
        public int TicketTypeId { get; set; }
        public int EntityTypeId { get; set; }
        public int SortOrder { get; set; }
        public int DisplayMode { get; set; }
        public string BackgroundColor { get; set; }
        public string BackgroundImage { get; set; }
        public int FontSize { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public string DisplayState { get; set; }
        public string StateFilter { get; set; }

        private readonly IList<EntityScreenMap> _entityScreenMaps;
        public virtual IList<EntityScreenMap> EntityScreenMaps
        {
            get { return _entityScreenMaps; }
        }

        private readonly IList<EntityScreenItem> _screenItems;
        public virtual IList<EntityScreenItem> ScreenItems
        {
            get { return _screenItems; }
        }

        private readonly IList<Widget> _widgets;
        public virtual IList<Widget> Widgets
        {
            get { return _widgets; }
        }

        public string UserString
        {
            get { return Name; }
        }

        public bool IsBackgroundImageVisible { get { return !string.IsNullOrEmpty(BackgroundImage); } }

        public EntityScreen()
        {
            _entityScreenMaps = new List<EntityScreenMap>();
            _screenItems = new List<EntityScreenItem>();
            _widgets = new List<Widget>();
            BackgroundColor = "Transparent";
            PageCount = 1;
            ButtonHeight = 0;
        }

        public int ItemCountPerPage
        {
            get
            {
                var itemCount = ScreenItems.Count / PageCount;
                if (ScreenItems.Count % PageCount > 0) itemCount++;
                return itemCount;
            }
        }

        public void AddScreenItem(EntityScreenItem choosenValue)
        {
            if (!ScreenItems.Contains(choosenValue))
                ScreenItems.Add(choosenValue);
        }
    }
}
