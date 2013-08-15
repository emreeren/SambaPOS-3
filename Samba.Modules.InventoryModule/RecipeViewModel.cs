using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using System.Linq;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class RecipeViewModel : EntityViewModelBase<Recipe>
    {
        private readonly IInventoryService _inventoryService;
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public RecipeViewModel(IInventoryService inventoryService, IMenuService menuService)
        {
            _inventoryService = inventoryService;
            _menuService = menuService;

            AddInventoryItemCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Inventory), OnAddInventoryItem, CanAddInventoryItem);
            DeleteInventoryItemCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Inventory), OnDeleteInventoryItem, CanDeleteInventoryItem);
        }

        public override Type GetViewType()
        {
            return typeof(RecipeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Recipe;
        }

        public ICaptionCommand AddInventoryItemCommand { get; set; }
        public ICaptionCommand DeleteInventoryItemCommand { get; set; }

        private ObservableCollection<RecipeItemViewModel> _recipeItems;
        public ObservableCollection<RecipeItemViewModel> RecipeItems
        {
            get { return _recipeItems ?? (_recipeItems = new ObservableCollection<RecipeItemViewModel>(Model.RecipeItems.Select(x => new RecipeItemViewModel(x, Workspace, _inventoryService)))); }
        }

        private RecipeItemViewModel _selectedRecipeItem;
        public RecipeItemViewModel SelectedRecipeItem
        {
            get { return _selectedRecipeItem; }
            set
            {
                _selectedRecipeItem = value;
                RaisePropertyChanged(() => SelectedRecipeItem);
            }
        }

        private string _selectedMenuItemName;
        public string SelectedMenuItemName
        {
            get { return _selectedMenuItemName; }
            set
            {
                _selectedMenuItemName = value;
                if (SelectedMenuItem == null || SelectedMenuItem.Name != value)
                {
                    Portion = null;
                    var miName = _selectedMenuItemName.ToLower();
                    var mi = Workspace.Single<MenuItem>(x => x.Name.ToLower() == miName);
                    SelectedMenuItem = mi;
                    if (mi != null && mi.Portions.Count == 1)
                        Portion = mi.Portions[0];
                }
                RaisePropertyChanged(() => SelectedMenuItemName);
            }
        }

        private IEnumerable<string> _menuItemNames;
        public IEnumerable<string> MenuItemNames
        {
            get { return _menuItemNames ?? (_menuItemNames = _menuService.GetMenuItemNames()); }
        }

        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem
        {
            get { return GetMenuItem(); }
            set
            {
                _selectedMenuItem = value;
                if (value != null)
                { SelectedMenuItemName = value.Name; }
                else Portion = null;
                RaisePropertyChanged(() => SelectedMenuItem);
            }
        }

        private MenuItem GetMenuItem()
        {
            if (_selectedMenuItem == null)
            {
                if (Model.Portion != null)
                    SelectedMenuItem = Workspace.Single<MenuItem>(x => x.Id == Model.Portion.MenuItemId);
            }
            return _selectedMenuItem;
        }

        public MenuItemPortion Portion
        {
            get { return Model.Portion; }
            set
            {
                Model.Portion = value;
                RaisePropertyChanged(() => Portion);
            }
        }

        public decimal FixedCost
        {
            get { return Model.FixedCost; }
            set { Model.FixedCost = value; }
        }

        private void OnDeleteInventoryItem(string obj)
        {
            if (SelectedRecipeItem != null)
            {
                if (SelectedRecipeItem.Model.Id > 0)
                    Workspace.Delete(SelectedRecipeItem.Model);
                Model.RecipeItems.Remove(SelectedRecipeItem.Model);
                RecipeItems.Remove(SelectedRecipeItem);
            }
        }

        private void OnAddInventoryItem(string obj)
        {
            var ri = new RecipeItem();
            Model.RecipeItems.Add(ri);
            var riv = new RecipeItemViewModel(ri, Workspace, _inventoryService);
            RecipeItems.Add(riv);
            SelectedRecipeItem = riv;
        }

        private bool CanAddInventoryItem(string arg)
        {
            return Portion != null;
        }

        private bool CanDeleteInventoryItem(string arg)
        {
            return SelectedRecipeItem != null;
        }
    }
}
