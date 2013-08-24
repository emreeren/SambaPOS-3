using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using FluentValidation;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
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
            EditMenuPropertiesCommand = new CaptionCommand<string>(string.Format(Resources.Edit_f, Resources.MenuProperties), OnEditMenuProperties);
        }

        [Browsable(false)]
        public ICaptionCommand AddCategoryCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand EditCategoryCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand EditAllCategoriesCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand EditCategoryItemsCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand DeleteCategoryCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand SortCategoryItemsCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand SortCategoriesCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand EditCategoryItemPropertiesCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand EditMenuPropertiesCommand { get; set; }

        public int CategoryColumnCount
        {
            get { return Model.CategoryColumnCount > 1 ? Model.CategoryColumnCount : 1; }
            set { Model.CategoryColumnCount = value > 1 ? value : 1; }
        }

        public int CategoryColumnWidthRate
        {
            get { return Model.CategoryColumnWidthRate > 10 ? Model.CategoryColumnWidthRate : 10; }
            set { Model.CategoryColumnWidthRate = value > 10 ? value : 10; }
        }

        [Browsable(false)]
        public ObservableCollection<ScreenMenuCategoryViewModel> Categories { get; set; }

        [Browsable(false)]
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
            return baseModel.Categories.Select(item => new ScreenMenuCategoryViewModel(item)).OrderBy(x => x.Model.SortOrder).ToList();
        }

        private void OnAddCategory(string value)
        {
            var values =
                InteractionService.UserIntraction
                    .GetStringFromUser(Resources.Categories, Resources.AddCategoryHint)
                    .Where(x => !Categories.Select(y => y.Name).Contains(x)).Distinct().ToList();

            foreach (string val in values.Where(x => Model.Categories.All(y => y.Name != x.Trim())))
            {
                Categories.Add(new ScreenMenuCategoryViewModel(Model.AddCategory(val)));
            }

            if (values.Any())
            {
                var answer = InteractionService.UserIntraction.AskQuestion(
                        Resources.AutoSelectProductsQuestion);
                if (answer)
                {
                    foreach (var val in values)
                    {
                        var menuItems = GetMenuItemsByGroupCode(val);
                        if (menuItems.Any())
                        {
                            Model.AddItemsToCategory(val, menuItems.ToList());
                        }
                    }
                }
            }
        }

        private IEnumerable<MenuItem> GetMenuItemsByGroupCode(string groupCode)
        {
            return Workspace.All<MenuItem>(x => x.GroupCode == groupCode);
        }

        private void OnEditMenuProperties(string obj)
        {
            InteractionService.UserIntraction.EditProperties(this);
        }

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
            InteractionService.UserIntraction.EditProperties(SelectedCategory.ScreenMenuItems.Select(x => new ScreenMenuItemViewModel(x)).ToList());
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
                if (SelectedCategory.Model.Id > 0)
                    Workspace.Delete(SelectedCategory.Model);
                Model.Categories.Remove(SelectedCategory.Model);
                Categories.Remove(SelectedCategory);
            }
        }

        private void OnSortCategoryItems(string obj)
        {
            InteractionService.UserIntraction.SortItems(SelectedCategory.ScreenMenuItems.OrderBy(x => x.SortOrder), Resources.SortCategoryProducts,
                string.Format(Resources.SortCategoryProductsDialogHint_f, SelectedCategory.Name));
        }

        private void OnSortCategories(string obj)
        {
            InteractionService.UserIntraction.SortItems(Model.Categories, Resources.SortCategories,
                string.Format(Resources.SortCategoriesDialogHint_f, Model.Name));
            Categories = new ObservableCollection<ScreenMenuCategoryViewModel>(Categories.OrderBy(x => x.Model.SortOrder));
            RaisePropertyChanged(() => Categories);
        }

        private bool CanSortCategories(string arg)
        {
            return Categories.Count > 1;
        }

        protected override AbstractValidator<ScreenMenu> GetValidator()
        {
            return new ScreenMenuValidatior();
        }

        protected override void OnSave(string value)
        {
            foreach (var screenMenuCategoryViewModel in Categories)
            {
                foreach (var smi in screenMenuCategoryViewModel.ScreenMenuItems)
                {
                    foreach (var propertyInfo in smi.GetType().GetProperties())
                    {
                        var val = propertyInfo.GetValue(smi, null);
                        if (val is string && val.ToString().Contains('\b') && propertyInfo.CanWrite)
                            propertyInfo.SetValue(smi, "", null);
                    }
                }
            }
            base.OnSave(value);
        }
    }

    internal class ScreenMenuValidatior : AbstractValidator<ScreenMenu>
    {
        public ScreenMenuValidatior()
        {
            RuleFor(x => x.Categories)
                .Must(x => x.GroupBy(y => y.Name).All(y => y.Count() == 1)).WithMessage("All Category Names should be unique.");
        }
    }
}
