using System.Collections.Generic;
using Samba.Domain.Foundation;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class MenuItemPortion : IEntity
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public Price Price { get; set; }
        public int Multiplier { get; set; }

        private IList<MenuItemPrice> _prices;
        public virtual IList<MenuItemPrice> Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }

        public MenuItemPortion()
        {
            Multiplier = 1;
            _prices = new List<MenuItemPrice>();
        }
    }
}
