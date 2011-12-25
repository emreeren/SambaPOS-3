using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.UserModule
{
    public class UserRoleListViewModel : EntityCollectionViewModelBase<UserRoleViewModel, UserRole>
    {
        protected override string CanDeleteItem(UserRole model)
        {
            var count = Dao.Count<User>(x => x.UserRole.Id == model.Id);
            if (count > 0) return Resources.DeleteErrorThisRoleUsedInAUserAccount;
            return base.CanDeleteItem(model);
        }
    }
}
