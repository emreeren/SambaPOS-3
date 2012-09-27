using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.MenuModule
{
    public class ScreenMenuViewModel : EntityViewModelBase<ScreenMenu>
    {
        public ScreenMenuViewModel()
        {
            AddCategoryCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Category), OnAddCategory);
            EditCategoryCommand = new CaptionCommand<string>(string.Format(Resources.Edit_f, Resources.Category), OnEditCategory, CanEditCategory);
            DeleteCategoryCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Category), OnDeleteCategory, CanEditCategory);
            EditCategoryItemsCommand = new CaptionCommand<string>(string.Format(Resources.Edit_f, Resources.CategoryProducts), OnEditCategoryItems, CanEditCategory);
            SortCategoryItemsCommand = new CaptionCommand<string>(string.Format(Resources.SortCategoryProducts), OnSortCategoryItems, CanEditCategory);
            SortCategoriesCommand = new CaptionCommand<string>(string.Format(Resources.SortCategories), OnSortCategories, CanSortCategories);
            EditCategoryItemPropertiesCommand = new CaptionCommand<string>(string.Format(Resources.Edit_f, Resources.ProductProperties), OnEditCategoryItemProperties, CanEditCategory);
            EditAllCategoriesCommand = new CaptionCommand<string>(string.Format(Resources.Edit_f, Resources.AllCategories), OnEditAllCategories);
        }

        public ICaptionCommand AddCategoryCommand { get; set; }
        public ICaptionCommand EditCategoryCommand { get; set; }
        public ICaptionCommand EditAllCategoriesCommand { get; set; }
        public ICaptionCommand EditCategoryItemsCommand { get; set; }
        public ICaptionCommand DeleteCategoryCommand { get; set; }
        public ICaptionCommand SortCategoryItemsCommand { get; set; }
        public ICaptionCommand SortCategoriesCommand { get; set; }
        public ICaptionCommand EditCategoryItemPropertiesCommand { get; set; }

        public ObservableCollection<ScreenMenuCategoryViewModel> Categories { get; set; }
        public ScreenMenuCategoryViewModel SelectedCategory { get; set; }

        public override string GetModelTypeString()
        {
            return Resources.Menu;
        }

        protected override void Initialize()
        {
            Categories = new ObservableCollection<ScreenMenuCategoryViewModel>(GetCategories(Model));
        }

        public override Type GetViewType()
        {
            return typeof(ScreenMenuView);
        }

        private static IEnumerable<ScreenMenuCategoryViewModel> GetCategories(ScreenMenu baseModel)
        {
            return baseModel.Categories.Select(item => new ScreenMenuCategoryViewModel(item)).OrderBy(x => x.Model.Order).ToList();
        }

        private void OnAddCategory(string value)
        {
            string[] values = InteractionService.UserIntraction.GetStringFromUser(Resources.Categories, Resources.AddCategoryHint);
            foreach (string val in values)
            {
                Categories.Add(new ScreenMenuCategoryViewModel(Model.AddCategory(val)));
            }
            if (values.Any())
            {
                bool answer = InteractionService.UserIntraction.AskQuestion(
                        Resources.AutoSelectProductsQuestion);
                if (answer)
                {
                    foreach (var val in values)
                    {
                        //TODO EF ile çalışırken tolist yapmazsak count sql sorgusu üretiyor mu kontrol et.
                        var menuItems = GetMenuItemsByGroupCode(val).ToList();
                        if (menuItems.Count > 0)
                        {
                            Model.AddItemsToCategory(val, menuItems);
                        }
                    }
                }
            }
        }

        private IEnumerable<MenuItem> GetMenuItemsByGroupCode(string groupCode)
        { return Workspace.All<MenuItem>(x => x.GroupCode == groupCode); }

        private bool CanEditCategory(string value)
        {
            return SelectedCategory != null;
        }

        private void OnEditAllCategories(string obj)
        {
            InteractionService.UserIntraction.EditProperties(Categories);
        }

        private void OnEditCategory(string obj)
        {
            InteractionService.UserIntraction.EditProperties(SelectedCategory);
        }

        private void OnEditCategoryItemProperties(string obj)
        {
            InteractionService.UserIntraction.EditProperties(SelectedCategory.ScreenMenuItems.Select(x => new ScreenMenuItemViewModel(Workspace, x)).ToList());
        }

        private void OnEditCategoryItems(string value)
        {
            if (SelectedCategory != null)
            {
                IList<IOrderable> values = new List<IOrderable>(Workspace.All<MenuItem>().OrderBy(x => x.GroupCode + x.Name)
                    .Where(x => !SelectedCategory.ContainsMenuItem(x))
                    .Select(x => new ScreenMenuItem { MenuItemId = x.Id, Name = x.Name, MenuItem = x }));

                IList<IOrderable> selectedValues = new List<IOrderable>(SelectedCategory.ScreenMenuItems);

                var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, Resources.ProductList,
                    string.Format(Resources.AddProductsToCategoryHint_f, SelectedCategory.Name), Resources.Product, Resources.Products);

                foreach (var screenMenuItem in SelectedCategory.ScreenMenuItems.ToList())
                {
                    if (!choosenValues.Contains(screenMenuItem) && screenMenuItem.Id > 0)
                        Workspace.Delete(screenMenuItem);
                }

                SelectedCategory.ScreenMenuItems.Clear();

                foreach (ScreenMenuItem item in choosenValues)
                {
                    SelectedCategory.ScreenMenuItems.Add(item);
                }

                SelectedCategory.UpdateDisplay();
            }
        }

        private void OnDeleteCategory(string value)
        {
            if (MessageBox.Show(Resources.DeleteSelectedCategoryQuestion, Resources.Confirmation, MessageBoxButton.YesNo) == MessageBoxResult.Yes
                && SelectedCategory != null)
            {
                Workspace.Delete(SelectedCategory.Model);
                Model.Categories.Remove(SelectedCategory.Model);
                Categories.Remove(SelectedCategory);
            }
        }

        private void OnSortCategoryItems(string obj)
        {
            InteractionService.UserIntraction.SortItems(SelectedCategory.ScreenMenuItems.OrderBy(x => x.Order), Resources.SortCategoryProducts,
                string.Format(Resources.SortCategoryProductsDialogHint_f, SelectedCategory.Name));
        }

        private void OnSortCategories(string obj)
        {
            InteractionService.UserIntraction.SortItems(Model.Categories, Resources.SortCategories,
                string.Format(Resources.SortCategoriesDialogHint_f, Model.Name));
            Categories = new ObservableCollection<ScreenMenuCategoryViewModel>(Categories.OrderBy(x => x.Model.Order));
            RaisePropertyChanged(() => Categories);
        }

        private bool CanSortCategories(string arg)
        {
            return Categories.Count > 1;
        }
    }
}
