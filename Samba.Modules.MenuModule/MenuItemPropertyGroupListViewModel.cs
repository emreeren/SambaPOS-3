using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class MenuItemPropertyGroupListViewModel : EntityCollectionViewModelBase<MenuItemPropertyGroupViewModel, MenuItemPropertyGroup>
    {
        protected override MenuItemPropertyGroupViewModel CreateNewViewModel(MenuItemPropertyGroup model)
        {
            return new MenuItemPropertyGroupViewModel(model);
        }

        protected override MenuItemPropertyGroup CreateNewModel()
        {
            return new MenuItemPropertyGroup();
        }

        protected override string CanDeleteItem(MenuItemPropertyGroup model)
        {
            var count = Dao.Query<MenuItem>(x => x.PropertyGroups.Select(y => y.Id).Contains(model.Id), x => x.PropertyGroups).Count();
            if (count > 0) return Resources.DeleteErrorProductPropertyUsedInProduct;
            return base.CanDeleteItem(model);
        }
    }
}
