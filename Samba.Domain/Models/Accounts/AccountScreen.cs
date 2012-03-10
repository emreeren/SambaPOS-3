using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountScreen : Entity, IOrderable
    {
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

        private IList<AccountScreenItem> _screenItems;
        public virtual IList<AccountScreenItem> ScreenItems
        {
            get { return _screenItems; }
            set { _screenItems = value; }
        }

        public string UserString
        {
            get { return Name; }
        }

        public bool IsBackgroundImageVisible { get { return !string.IsNullOrEmpty(BackgroundImage); } }

        public AccountScreen()
        {
            _screenItems = new List<AccountScreenItem>();
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
                var itemCount = ScreenItems.Count / PageCount;
                if (ScreenItems.Count % PageCount > 0) itemCount++;
                return itemCount;
            }
        }

        public void AddScreenItem(AccountScreenItem choosenValue)
        {
            if (!ScreenItems.Contains(choosenValue))
                ScreenItems.Add(choosenValue);
        }
    }
}
