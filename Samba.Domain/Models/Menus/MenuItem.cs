using System;
using System.Collections.Generic;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;

namespace Samba.Domain.Models.Menus
{
    public class MenuItem : EntityClass
    {
        public MenuItem()
            : this(string.Empty)
        {

        }

        public MenuItem(string name)
        {
            Name = name;
            _portions = new List<MenuItemPortion>();
        }

        public string GroupCode { get; set; }
        public string Barcode { get; set; }
        public string Tag { get; set; }

        private IList<MenuItemPortion> _portions;
        public virtual IList<MenuItemPortion> Portions
        {
            get { return _portions; }
            set { _portions = value; }
        }

        private static MenuItem _all;
        public static MenuItem All { get { return _all ?? (_all = new MenuItem { Name = "*" }); } }

        public MenuItemPortion AddPortion(string portionName, decimal price, string currencyCode)
        {
            var mip = new MenuItemPortion
            {
                Name = portionName,
                Price = price,
                MenuItemId = Id
            };
            Portions.Add(mip);
            return mip;
        }

        internal MenuItemPortion GetPortion(string portionName)
        {
            foreach (var portion in Portions)
            {
                if (portion.Name == portionName)
                    return portion;
            }
            if (string.IsNullOrEmpty(portionName) && Portions.Count > 0) return Portions[0];
            throw new Exception("Portion not found.");
        }

        public string UserString
        {
            get { return string.Format("{0} [{1}]", Name, GroupCode); }
        }

        public static MenuItemPortion AddDefaultMenuPortion(MenuItem item)
        {
            return item.AddPortion("Normal", 0, LocalSettings.CurrencySymbol);
        }

        public static OrderTag AddDefaultMenuItemProperty(OrderTagGroup item)
        {
            return item.AddOrderTag("", 0);
        }

        public static MenuItem Create()
        {
            var result = new MenuItem();
            AddDefaultMenuPortion(result);
            return result;
        }
    }
}
