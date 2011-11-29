using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.TicketModule
{
    public class MenuItemData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupCode { get; set; }
    }

    public class OrderTagMapViewModel : ObservableObject
    {
        protected internal OrderTagMap Model { get; set; }
        private const string NullLabel = "*";

        public OrderTagMapViewModel(OrderTagMap model)
        {
            Model = model;
        }

        public int Id { get { return Model.Id; } }

        public string MenuItemGroupCodeLabel
        {
            get { return string.IsNullOrEmpty(MenuItemGroupCode) ? NullLabel : MenuItemGroupCode; }
        }

        public string MenuItemGroupCode
        {
            get { return Model.MenuItemGroupCode ?? NullLabel; }
            set
            {
                Model.MenuItemGroupCode = value;
                MenuItemId = 0;
                RaisePropertyChanged(() => MenuItemGroupCode);
                RaisePropertyChanged(() => MenuItemGroupCodeLabel);
                RaisePropertyChanged(() => MenuItems);
            }
        }

        public string MenuItemLabel { get { return MenuItemId > 0 ? AllMenuItems.Single(x => x.Id == MenuItemId).Name : NullLabel; } }

        public int MenuItemId
        {
            get { return Model.MenuItemId; }
            set
            {
                Model.MenuItemId = value;
                RaisePropertyChanged(() => MenuItemId);
                RaisePropertyChanged(() => MenuItemLabel);
            }
        }

        public IEnumerable<MenuItemData> MenuItems { get { return GetAllMenuItems(MenuItemGroupCode); } }
        public IEnumerable<string> MenuItemGroupCodes { get { return GetAllMenuItemGroupCodes(); } }

        private IEnumerable<MenuItemData> _allMenuItems;
        public IEnumerable<MenuItemData> AllMenuItems
        {
            get { return _allMenuItems ?? (_allMenuItems = Dao.Select<MenuItem, MenuItemData>(x => new MenuItemData { Id = x.Id, GroupCode = x.GroupCode, Name = x.Name }, x => x.Id > 0).OrderBy(x => x.Name)); }
        }

        private static IEnumerable<string> GetAllMenuItemGroupCodes()
        {
            IList<string> result = new List<string>(Dao.Distinct<MenuItem>(x => x.GroupCode).OrderBy(x => x));
            result.Insert(0, NullLabel);
            return result;
        }

        private IEnumerable<MenuItemData> GetAllMenuItems(string groupCode)
        {
            IList<MenuItemData> result = string.IsNullOrEmpty(groupCode) || groupCode == NullLabel
                                         ? AllMenuItems.ToList()
                                         : AllMenuItems.Where(x => x.GroupCode == groupCode).ToList();
            result.Insert(0, new MenuItemData { Name = NullLabel });
            return result;
        }

    }
}
