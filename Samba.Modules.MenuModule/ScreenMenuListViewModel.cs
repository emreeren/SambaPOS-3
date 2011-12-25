using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class ScreenMenuListViewModel : EntityCollectionViewModelBase<ScreenMenuViewModel, ScreenMenu>
    {
        protected override string CanDeleteItem(ScreenMenu model)
        {
            var count = Dao.Count<Department>(x=>x.ScreenMenuId == model.Id);
            if (count > 0) return Resources.DeleteErrorMenuViewUsedInDepartment;
            return base.CanDeleteItem(model);
        }
    }
}
