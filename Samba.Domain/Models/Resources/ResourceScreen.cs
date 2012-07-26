using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceScreen : Entity, IOrderable
    {
        public int ResourceTemplateId { get; set; }
        public int Order { get; set; }
        public int DisplayMode { get; set; }
        public string BackgroundColor { get; set; }
        public string BackgroundImage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public int StateFilterId { get; set; }
        public bool DisplayOpenTickets { get; set; }

        private readonly IList<ResourceScreenItem> _screenItems;
        public virtual IList<ResourceScreenItem> ScreenItems
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

        public ResourceScreen()
        {
            _screenItems = new List<ResourceScreenItem>();
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
        
        public void AddScreenItem(ResourceScreenItem choosenValue)
        {
            if (!ScreenItems.Contains(choosenValue))
                ScreenItems.Add(choosenValue);
        }
    }
}
