using System.ComponentModel.Composition;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ScreenMenuListViewModel : EntityCollectionViewModelBase<ScreenMenuViewModel, ScreenMenu>
    {
    }
}
