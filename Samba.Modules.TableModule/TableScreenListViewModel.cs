using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TableModule
{
    public class TableScreenListViewModel : EntityCollectionViewModelBase<TableScreenViewModel, TableScreen>
    {
        protected override TableScreenViewModel CreateNewViewModel(TableScreen model)
        {
            return new TableScreenViewModel(model);
        }

        protected override TableScreen CreateNewModel()
        {
            return new TableScreen();
        }

        protected override string CanDeleteItem(TableScreen model)
        {
            var count = Dao.Count<Department>(x => x.TableScreenId == model.Id);
            if (count > 0) return Resources.DeleteErrorTableViewUsedInDepartment;
            return base.CanDeleteItem(model);
        }
    }
}
