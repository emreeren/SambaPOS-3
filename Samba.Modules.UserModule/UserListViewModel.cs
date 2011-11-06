using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.UserModule
{
    public class UserListViewModel : EntityCollectionViewModelBase<UserViewModel, User>
    {
        protected override UserViewModel CreateNewViewModel(User model)
        {
            return new UserViewModel(model);
        }

        protected override User CreateNewModel()
        {
            return new User();
        }

        protected override string CanDeleteItem(User model)
        {
            if (model.UserRole.IsAdmin) return Resources.DeleteErrorAdminUser;
            if (Workspace.Count<User>() == 1) return Resources.DeleteErrorLastUser;
            var ti = Dao.Count<TicketItem>(x => x.CreatingUserId == model.Id || x.ModifiedUserId == model.Id);
            if (ti > 0) return Resources.DeleteErrorUserDidTicketOperation;
            return base.CanDeleteItem(model);
        }
    }
}
