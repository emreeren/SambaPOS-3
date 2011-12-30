using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    public class MenuItemListViewModel : EntityCollectionViewModelBase<MenuItemViewModel, MenuItem>
    {
        public MenuItemListViewModel()
        {
            CreateBatchMenuItems = new CaptionCommand<string>(Resources.BatchCreteProducts, OnCreateBatchMenuItems);
            CustomCommands.Add(CreateBatchMenuItems);
        }

        public ICaptionCommand CreateBatchMenuItems { get; set; }

        private void OnCreateBatchMenuItems(string value)
        {
            var values = InteractionService.UserIntraction.GetStringFromUser(
                Resources.BatchCreteProducts,
                Resources.BatchCreateProductsDialogHint);

            var createdItems = new DataCreationService().BatchCreateMenuItems(values, Workspace);

            Workspace.CommitChanges();

            foreach (var createdItem in createdItems)
            {
                var mv = CreateNewViewModel(createdItem);
                mv.Init(Workspace, createdItem);
                Items.Add(mv);
            }
        }

        protected override System.Collections.Generic.IEnumerable<MenuItem> SelectItems()
        {
            return Workspace.All<MenuItem>();
        }
    }
}
