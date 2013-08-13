using System.Collections.Generic;
using Samba.Domain.Models.Menus;

namespace Samba.Domain.Builders
{
    public class MenuItemBuilder
    {
        private readonly string _menuItemName;
        private readonly List<PortionData> _portions;
        private string _groupCode;
        private string _productTag;
        private int _id;

        public MenuItemBuilder(string menuItemName)
        {
            _menuItemName = menuItemName;
            _portions = new List<PortionData>();
        }

        public static MenuItemBuilder Create(string menuItemName)
        {
            return new MenuItemBuilder(menuItemName);
        }

        public MenuItem Build()
        {
            var result = new MenuItem(_menuItemName) { Id = _id, GroupCode = _groupCode, Tag = _productTag };
            foreach (var portionData in _portions)
            {
                result.AddPortion(portionData.Name, portionData.Price, "");
            }
            return result;
        }

        public MenuItemBuilder AddPortion(string portionName, decimal price)
        {
            _portions.Add(new PortionData { Name = portionName, Price = price });
            return this;
        }

        public MenuItemBuilder WithGroupCode(string groupCode)
        {
            _groupCode = groupCode;
            return this;
        }

        public MenuItemBuilder WithProductTag(string productTag)
        {
            _productTag = productTag;
            return this;
        }

        public MenuItemBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
    }

    internal class PortionData
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}