using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Builders
{
    public class OrderBuilder
    {
        private MenuItem _menuItem;
        private MenuItemPortion _portion;
        private string _userName;
        private string _priceTag;
        private int _quantity;
        private Department _department;

        public static OrderBuilder Create()
        {
            return new OrderBuilder();
        }

        public OrderBuilder()
        {
            _priceTag = "";
            _quantity = 1;
        }

        public Order Build()
        {
            var result = new Order();
            result.UpdateMenuItem(_userName, _menuItem, null, _portion, _priceTag, _quantity);
            result.DepartmentId = _department.Id;
            result.WarehouseId = _department.WarehouseId;
            return result;
        }

        public OrderBuilder ForMenuItem(MenuItem menuItem)
        {
            _menuItem = menuItem;
            if (menuItem.Portions.Any() && _portion == null)
                _portion = menuItem.Portions[0];
            return this;
        }

        public OrderBuilder WithPortion(MenuItemPortion portion)
        {
            _portion = portion;
            return this;
        }

        public OrderBuilder WithUserName(string userName)
        {
            _userName = userName;
            return this;
        }

        public OrderBuilder WithPriceTag(string priceTag)
        {
            _priceTag = priceTag;
            return this;
        }

        public OrderBuilder WithQuantity(int quantity)
        {
            _quantity = quantity;
            return this;
        }

        public OrderBuilder WithDepartment(Department department)
        {
            _department = department;
            return this;
        }
    }
}