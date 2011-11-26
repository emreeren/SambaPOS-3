using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Presentation.Common;

namespace Samba.Presentation.Terminal
{
    public class MenuItemSelectorViewModel : ObservableObject
    {
        public event OrderSelectedEventHandler OnOrderSelected;
        public void InvokeOnOrderSelected(OrderViewModel order)
        {
            var handler = OnOrderSelected;
            if (handler != null) handler(order);
        }

        public ScreenMenu CurrentScreenMenu { get; set; }
        public ScreenMenuCategory MostUsedItemsCategory { get; set; }
        public ObservableCollection<ScreenMenuItemButton> MostUsedMenuItems { get; set; }
        public ObservableCollection<ScreenCategoryButton> Categories { get; set; }
        public ObservableCollection<ScreenMenuItemButton> MenuItems { get; set; }
        public DelegateCommand<ScreenMenuCategory> CategorySelectionCommand { get; set; }
        public DelegateCommand<ScreenMenuItem> MenuItemSelectionCommand { get; set; }
        public DelegateCommand<OrderViewModel> ItemSelectedCommand { get; set; }

        public ObservableCollection<OrderViewModel> AddedMenuItems { get; set; }

        private ScreenMenuCategory _selectedCategory;
        public ScreenMenuCategory SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                if (IsQuickNumeratorVisible)
                {
                    QuickNumeratorValues = string.IsNullOrEmpty(value.NumeratorValues) ? new[] { "1", "2", "3", "4", "5" } : value.NumeratorValues.Split(',');
                    NumeratorValue = QuickNumeratorValues[0];
                }
                else NumeratorValue = "";
                RaisePropertyChanged(() => IsQuickNumeratorVisible);
                RaisePropertyChanged(() => QuickNumeratorValues);
                RaisePropertyChanged(() => IsPageNumberNavigatorVisible);
                RaisePropertyChanged(() => SelectedCategory);
            }
        }

        private string _numeratorValue;
        public string NumeratorValue
        {
            get { return _numeratorValue; }
            set { _numeratorValue = value; RaisePropertyChanged(() => NumeratorValue); }
        }
        public string[] QuickNumeratorValues { get; set; }

        public bool IsQuickNumeratorVisible { get { return SelectedCategory != null && SelectedCategory.IsQuickNumeratorVisible; } }
        public bool IsPageNumberNavigatorVisible { get { return SelectedCategory != null && SelectedCategory.PageCount > 1; } }
        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }

        public int CurrentPageNo { get; set; }
        public string CurrentTag { get; set; }

        public MenuItemSelectorViewModel()
        {
            MostUsedMenuItems = new ObservableCollection<ScreenMenuItemButton>();
            Categories = new ObservableCollection<ScreenCategoryButton>();
            MenuItems = new ObservableCollection<ScreenMenuItemButton>();
            AddedMenuItems = new ObservableCollection<OrderViewModel>();

            CategorySelectionCommand = new DelegateCommand<ScreenMenuCategory>(OnCategorySelected);
            MenuItemSelectionCommand = new DelegateCommand<ScreenMenuItem>(OnMenuItemSelected);
            ItemSelectedCommand = new DelegateCommand<OrderViewModel>(OnItemSelected);
            IncPageNumberCommand = new CaptionCommand<string>(Resources.Next + " >>", OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>("<< " + Resources.Previous, OnDecPageNumber, CanDecPageNumber);
        }

        private void OnDecPageNumber(string obj)
        {
            CurrentPageNo--;
            UpdateMenuButtons(SelectedCategory);
        }

        private bool CanDecPageNumber(string arg)
        {
            return CurrentPageNo > 0;
        }

        private bool CanIncPageNumber(object arg)
        {
            return SelectedCategory != null && CurrentPageNo < SelectedCategory.PageCount - 1;
        }

        private void OnIncPageNumber(object obj)
        {
            CurrentPageNo++;
            UpdateMenuButtons(SelectedCategory);
        }

        private void OnItemSelected(OrderViewModel obj)
        {
            InvokeOnOrderSelected(obj);
        }

        private void OnMenuItemSelected(ScreenMenuItem obj)
        {
            if (DataContext.SelectedTicket.IsLocked && !AppServices.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets)) return;

            decimal selectedMultiplier = 1;
            if (!string.IsNullOrEmpty(NumeratorValue))
                decimal.TryParse(NumeratorValue, out selectedMultiplier);

            if (IsQuickNumeratorVisible)
                NumeratorValue = QuickNumeratorValues[0];

            if (selectedMultiplier > 0)
            {
                if ((QuickNumeratorValues == null || selectedMultiplier.ToString() == QuickNumeratorValues[0]) && obj.Quantity > 1)
                    selectedMultiplier = obj.Quantity;
            }

            var item = DataContext.SelectedTicket.AddNewItem(obj.MenuItemId, selectedMultiplier, obj.ItemPortion, obj.OrderTagTemplate);
            if (item != null)
                AddedMenuItems.Add(item);
            if (obj.AutoSelect)
                InvokeOnOrderSelected(item);
        }

        private void OnCategorySelected(ScreenMenuCategory obj)
        {
            UpdateMenuButtons(obj);
        }

        private void UpdateMenuButtons(ScreenMenuCategory category)
        {
            SelectedCategory = category;
            CreateMenuItemButtons(MenuItems, category, CurrentPageNo, CurrentTag);
        }

        public void Refresh()
        {
            AddedMenuItems.Clear();
            if (CurrentScreenMenu == null)
            {
                CurrentScreenMenu = AppServices.MainDataContext.SelectedDepartment != null
                    ? AppServices.DataAccessService.GetScreenMenu(AppServices.MainDataContext.SelectedDepartment.TerminalScreenMenuId)
                    : null;

                if (CurrentScreenMenu != null)
                {
                    Categories.Clear();
                    Categories.AddRange(CurrentScreenMenu.Categories.OrderBy(x => x.Order)
                        .Where(x => !x.MostUsedItemsCategory)
                        .Select(x => new ScreenCategoryButton(x, CategorySelectionCommand)));
                    MostUsedItemsCategory = CurrentScreenMenu.Categories.FirstOrDefault(x => x.MostUsedItemsCategory);

                    if (MostUsedItemsCategory != null)
                        CreateMenuItemButtons(MostUsedMenuItems, MostUsedItemsCategory, CurrentPageNo, CurrentTag);

                    if (Categories.Count > 0)
                    {
                        UpdateMenuButtons(Categories[0].Category);
                    }

                }
            }
        }

        private void CreateMenuItemButtons(ObservableCollection<ScreenMenuItemButton> itemButtons,
            ScreenMenuCategory category, int pageNo, string tag)
        {
            itemButtons.Clear();
            if (category == null) return;
            itemButtons.AddRange(AppServices.DataAccessService.GetMenuItems(category, pageNo, tag)
                .Select(x => new ScreenMenuItemButton(x, MenuItemSelectionCommand, category)));
        }

        public void CloseView()
        {
            AddedMenuItems.Clear();

        }
    }
}
