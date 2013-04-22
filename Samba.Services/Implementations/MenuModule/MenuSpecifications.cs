using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data.Specification;

namespace Samba.Services.Implementations.MenuModule
{
    public static class MenuSpecifications
    {


        public static Specification<ScreenMenuItem> ScreenMenuItemsByMenuItemId(int menuItemId)
        {
            return new DirectSpecification<ScreenMenuItem>(x => x.MenuItemId == menuItemId);
        }

        public static Specification<Recipe> RecipesByMenuItemId(int menuItemId)
        {
            return new DirectSpecification<Recipe>(x => x.Portion.MenuItemId == menuItemId);
        }

        public static Specification<OrderTag> OrderTagsByMenuItemId(int menuItemId)
        {
            return new DirectSpecification<OrderTag>(x=>x.MenuItemId == menuItemId);
        }
    }
}
