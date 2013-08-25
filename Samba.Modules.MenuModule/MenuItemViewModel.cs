using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Collections.ObjectModel;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common.DataGeneration;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class MenuItemViewModel : EntityViewModelBase<MenuItem>, IEntityCreator<MenuItem>
    {
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public MenuItemViewModel(IMenuService menuService)
        {
            AddPortionCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Portion), OnAddPortion);
            DeletePortionCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Portion), OnDeletePortion, CanDeletePortion);
            _menuService = menuService;
        }

        private IEnumerable<string> _groupCodes;
        public IEnumerable<string> GroupCodes { get { return _groupCodes ?? (_groupCodes = _menuService.GetMenuItemGroupCodes()); } }

        private IEnumerable<string> _tags;
        public IEnumerable<string> Tags { get { return _tags ?? (_tags = _menuService.GetMenuItemTags()); } }

        private ObservableCollection<PortionViewModel> _portions;
        public ObservableCollection<PortionViewModel> Portions
        {
            get { return _portions ?? (_portions = new ObservableCollection<PortionViewModel>(GetPortions(Model))); }
        }

        public PortionViewModel SelectedPortion { get; set; }

        public ICaptionCommand AddPortionCommand { get; set; }
        public ICaptionCommand DeletePortionCommand { get; set; }

        public string GroupCode
        {
            get { return Model.GroupCode ?? ""; }
            set { Model.GroupCode = value; }
        }

        public string Tag
        {
            get { return Model.Tag ?? ""; }
            set { Model.Tag = value; }
        }

        public string Barcode
        {
            get { return Model.Barcode ?? ""; }
            set { Model.Barcode = value; }
        }

        public string GroupValue { get { return Model.GroupCode; } }

        private void OnAddPortion(string value)
        {
            var portion = MenuItem.AddDefaultMenuPortion(Model);
            Portions.Add(new PortionViewModel(portion));
            Workspace.Add(portion);
        }

        private void OnDeletePortion(string value)
        {
            if (SelectedPortion != null)
            {
                Workspace.Delete(SelectedPortion.Model);
                Model.Portions.Remove(SelectedPortion.Model);
                Portions.Remove(SelectedPortion);
            }
        }

        private bool CanDeletePortion(string value)
        {
            return SelectedPortion != null;
        }

        public override string GetModelTypeString()
        {
            return Resources.Product;
        }

        public override Type GetViewType()
        {
            return typeof(MenuItemView);
        }

        private static IEnumerable<PortionViewModel> GetPortions(MenuItem baseModel)
        {
            return baseModel.Portions.Select(item => new PortionViewModel(item));
        }

        public IEnumerable<MenuItem> CreateItems(IEnumerable<string> data)
        {
            return new DataCreationService().BatchCreateMenuItems(data.ToArray(), Workspace);
        }
    }
}
