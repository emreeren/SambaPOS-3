using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class MenuItemPropertyGroupViewModel : EntityViewModelBase<MenuItemPropertyGroup>
    {
        private readonly ObservableCollection<MenuItemPropertyViewModel> _properties;
        public ObservableCollection<MenuItemPropertyViewModel> Properties { get { return _properties; } }

        public MenuItemPropertyViewModel SelectedProperty { get; set; }
        public ICaptionCommand AddPropertyCommand { get; set; }
        public ICaptionCommand DeletePropertyCommand { get; set; }

        public bool SingleSelection { get { return Model.SingleSelection; } set { Model.SingleSelection = value; } }
        public bool MultipleSelection { get { return Model.MultipleSelection; } set { Model.MultipleSelection = value; } }
        public bool CalculateWithParentPrice { get { return Model.CalculateWithParentPrice; } set { Model.CalculateWithParentPrice = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int TerminalButtonHeight { get { return Model.TerminalButtonHeight; } set { Model.TerminalButtonHeight = value; } }
        public int TerminalColumnCount { get { return Model.TerminalColumnCount; } set { Model.TerminalColumnCount = value; } }

        public MenuItemPropertyGroupViewModel(MenuItemPropertyGroup model)
            : base(model)
        {
            _properties = new ObservableCollection<MenuItemPropertyViewModel>(GetProperties(model));
            AddPropertyCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Modifier), OnAddPropertyExecuted);
            DeletePropertyCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Modifier), OnDeletePropertyExecuted, CanDeleteProperty);
        }

        private void OnDeletePropertyExecuted(string obj)
        {
            if (SelectedProperty == null) return;
            if (SelectedProperty.Model.Id > 0)
                Workspace.Delete(SelectedProperty.Model);
            Model.Properties.Remove(SelectedProperty.Model);
            Properties.Remove(SelectedProperty);
        }

        private bool CanDeleteProperty(string arg)
        {
            return SelectedProperty != null;
        }

        private void OnAddPropertyExecuted(string obj)
        {
            Properties.Add(new MenuItemPropertyViewModel(MenuItem.AddDefaultMenuItemProperty(Model)));
        }

        private static IEnumerable<MenuItemPropertyViewModel> GetProperties(MenuItemPropertyGroup baseModel)
        {
            return baseModel.Properties.Select(item => new MenuItemPropertyViewModel(item));
        }

        public override string GetModelTypeString()
        {
            return Resources.ModifierGroup;
        }

        public override Type GetViewType()
        {
            return typeof(MenuItemPropertyGroupView);
        }
    }
}
