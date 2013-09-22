using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class MenuItemSelectorViewModel : ObservableObject
    {
        private ScreenMenu _currentScreenMenu;

        public ObservableCollection<ScreenMenuItemButton> MostUsedMenuItems { get; set; }
        public ObservableCollection<ScreenCategoryButton> Categories { get; set; }
        public ObservableCollection<ScreenMenuItemButton> MenuItems { get; set; }
        public ObservableCollection<ScreenSubCategoryButton> SubCategories { get; set; }
        public DelegateCommand<ScreenMenuCategory> CategoryCommand { get; set; }
        public DelegateCommand<ScreenMenuItem> MenuItemCommand { get; set; }
        public DelegateCommand<string> ClearNumeratorCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }
        public ICaptionCommand SubCategoryCommand { get; set; }
        public ICaptionCommand CloseMenuViewCommand { get; set; }

        public ScreenMenuCategory MostUsedItemsCategory { get; set; }

        private ScreenMenuCategory _selectedCategory;
        public ScreenMenuCategory SelectedCategory
        {
            get { return _selectedCategory; }
            set { _selectedCategory = value; RaisePropertyChanged(() => SelectedCategory); }
        }

        public int SubButtonRows
        {
            get
            {
                return SelectedCategory == null || SubCategories.Count == 1 || _selectedCategory.SubButtonRows < 2
                    ? 1 : SelectedCategory.SubButtonRows;
            }
        }

        public string QuickNumeratorValue
        {
            get { return !string.IsNullOrEmpty(NumeratorValue) ? NumeratorValue : QuickNumeratorValues.FirstOrDefault(); }
            set { NumeratorValue = value; }
        }

        public string NumeratorValue
        {
            get { return _applicationState.NumberPadValue ?? ""; }
            set
            {
                _applicationStateSetter.SetNumberpadValue(value);
                FilterMenuItems(_applicationState.NumberPadValue);
                RaisePropertyChanged(() => NumeratorValue);
                RaisePropertyChanged(() => IsNumberpadEditorVisible);
                RaisePropertyChanged(() => QuickNumeratorValue);
            }
        }

        public string[] QuickNumeratorValues { get; set; }
        public string[] AlphaButtonValues { get; set; }

        private bool _isSelectedItemsVisible;
        public bool IsSelectedItemsVisible { get { return _isSelectedItemsVisible; } set { _isSelectedItemsVisible = value; RaisePropertyChanged(() => IsSelectedItemsVisible); } }

        public bool IsQuickNumeratorVisible { get { return SelectedCategory != null && SelectedCategory.IsQuickNumeratorVisible; } }
        public bool IsNumeratorVisible { get { return SelectedCategory != null && SelectedCategory.IsNumeratorVisible; } }
        public bool IsNumberpadEditorVisible { get { return IsNumeratorVisible || (IsQuickNumeratorVisible && !string.IsNullOrEmpty(NumeratorValue) && QuickNumeratorValues.All(x => x != NumeratorValue)) || (!IsQuickNumeratorVisible && !string.IsNullOrEmpty(NumeratorValue)); } }
        public bool IsPageNumberNavigatorVisible { get { return SelectedCategory != null && SelectedCategory.PageCount > 1; } }
        public VerticalAlignment MenuItemsVerticalAlignment { get { return SelectedCategory != null && SelectedCategory.MenuItemButtonHeight > 0 ? VerticalAlignment.Top : VerticalAlignment.Stretch; } }
        public VerticalAlignment CategoriesVerticalAlignment { get { return Categories != null && Categories.Count > 0 && double.IsNaN(Categories[0].MButtonHeight) ? VerticalAlignment.Stretch : VerticalAlignment.Top; } }
        public int CurrentPageNo { get; set; }
        public string CurrentTag { get; set; }

        public ObservableCollection<ScreenMenuItemData> SelectedMenuItems { get; set; }

        public Ticket SelectedTicket { get; set; }

        public int CategoryColumnWidthRate
        {
            get
            {
                if (_currentScreenMenu == null) return 25;
                return _currentScreenMenu.CategoryColumnWidthRate > 10 ? _currentScreenMenu.CategoryColumnWidthRate : 10;
            }
        }
        public int CategoryColumnCount
        {
            get
            {
                if (_currentScreenMenu == null) return 1;
                return _currentScreenMenu.CategoryColumnCount > 1 ? _currentScreenMenu.CategoryColumnCount : 1;
            }
        }

        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ISettingService _settingService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public MenuItemSelectorViewModel(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ISettingService settingService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _settingService = settingService;
            _cacheService = cacheService;

            CategoryCommand = new DelegateCommand<ScreenMenuCategory>(OnCategoryCommandExecute);
            MenuItemCommand = new DelegateCommand<ScreenMenuItem>(OnMenuItemCommandExecute);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecute);
            ClearNumeratorCommand = new DelegateCommand<string>(OnClearNumeratorCommand);
            IncPageNumberCommand = new CaptionCommand<string>(Localization.Properties.Resources.NextPage + " >>", OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>("<< " + Localization.Properties.Resources.PreviousPage, OnDecPageNumber, CanDecPageNumber);
            SubCategoryCommand = new CaptionCommand<ScreenSubCategoryButton>(".", OnSubCategoryCommand);
            CloseMenuViewCommand = new CaptionCommand<string>(Localization.Properties.Resources.Close, OnCloseMenuView);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnEvent);
            NumeratorValue = "";
            QuickNumeratorValues = new[] { "1", "2", "3", "4", "5" };

            SubCategories = new ObservableCollection<ScreenSubCategoryButton>();
            SelectedMenuItems = new ObservableCollection<ScreenMenuItemData>();
        }

        private void OnClearNumeratorCommand(string obj)
        {
            NumeratorValue = "";
        }

        private bool _filtered;
        private void FilterMenuItems(string numeratorValue)
        {
            if (!string.IsNullOrEmpty(numeratorValue) && Char.IsLower(numeratorValue[0]) && MenuItems != null)
            {
                _filtered = true;
                SubCategories.Clear();
                MenuItems.Clear();
                var items = Categories.Select(x => x.Category).SelectMany(x => x.ScreenMenuItems).Where(
                    x => numeratorValue.ToLower().Split(' ').All(y => x.Name.ToLower().Contains(y)))
                    .Select(x => new ScreenMenuItemButton(x, MenuItemCommand, SelectedCategory));
                MenuItems.AddRange(items.OrderBy(x => x.FindOrder(numeratorValue)).Take(30));
            }
            else if (_filtered)
            {
                _filtered = false;
                UpdateMenuButtons(SelectedCategory);
            }
        }

        private void OnCloseMenuView(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnEvent(EventParameters<EventAggregator> obj)
        {
            switch (obj.Topic)
            {
                case EventTopicNames.ResetNumerator:
                    NumeratorValue = "";
                    break;
                case EventTopicNames.ActivateMenuView:
                    SelectedMenuItems.Clear();
                    break;
            }
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

        private void FindMenuItem()
        {
            var insertedData = NumeratorValue;
            decimal quantity = 1;
            var separator = _settingService.ProgramSettings.QuantitySeparators.Split(',').FirstOrDefault(x => NumeratorValue.Contains(x));
            if (!string.IsNullOrEmpty(separator))
            {
                insertedData = NumeratorValue.Substring(NumeratorValue.IndexOf(separator, StringComparison.Ordinal) + 1);
                var q = NumeratorValue.Substring(0, NumeratorValue.IndexOf(separator, StringComparison.Ordinal));
                decimal.TryParse(q, out quantity);
            }
            NumeratorValue = "";

            if (quantity <= 0) return;

            var weightBarcodePrefix = _settingService.ProgramSettings.WeightBarcodePrefix;
            if (!string.IsNullOrEmpty(weightBarcodePrefix) && insertedData.StartsWith(weightBarcodePrefix))
            {
                var itemLength = _settingService.ProgramSettings.WeightBarcodeItemLength;
                var quantityLength = _settingService.ProgramSettings.WeightBarcodeQuantityLength;
                if (itemLength > 0 && quantityLength > 0 && insertedData.Length >= itemLength + quantityLength + weightBarcodePrefix.Length)
                {
                    var bc = insertedData.Substring(weightBarcodePrefix.Length, itemLength);
                    if (!string.IsNullOrEmpty(_settingService.ProgramSettings.WeightBarcodeItemFormat))
                    {
                        int integerValue;
                        int.TryParse(bc, out integerValue);
                        if (integerValue > 0)
                            bc = integerValue.ToString(_settingService.ProgramSettings.WeightBarcodeItemFormat);
                    }
                    var qty = insertedData.Substring(weightBarcodePrefix.Length + itemLength, quantityLength);
                    if (bc.Length > 0 && qty.Length > 0)
                    {
                        insertedData = bc;
                        decimal.TryParse(qty, out quantity);
                    }
                }
            }

            var mi = _cacheService.FindMenuItemByBarcode(insertedData);
            if (mi != null)
            {
                var si = new ScreenMenuItem { MenuItemId = mi.Id, Name = mi.Name };
                var data = new ScreenMenuItemData { ScreenMenuItem = si, Quantity = quantity };
                data.PublishEvent(EventTopicNames.ScreenMenuItemDataSelected);
            }
            else
                _applicationState.NotifyEvent(RuleEventNames.NumberpadValueEntered, new { Ticket = SelectedTicket, NumberpadValue = insertedData });
        }

        private void OnMenuItemCommandExecute(ScreenMenuItem screenMenuItem)
        {
            decimal selectedMultiplier = 1;
            if (!string.IsNullOrEmpty(NumeratorValue) && !_filtered)
                decimal.TryParse(NumeratorValue, out selectedMultiplier);

            if (IsQuickNumeratorVisible)
                NumeratorValue = QuickNumeratorValues[0] != "1" ? QuickNumeratorValues[0] : "";
            if (IsNumberpadEditorVisible)
                NumeratorValue = "";

            if (selectedMultiplier > 0)
            {
                var data = new ScreenMenuItemData { ScreenMenuItem = screenMenuItem, Quantity = selectedMultiplier };
                if (data.Quantity == 1 && screenMenuItem.Quantity > 1)
                    data.Quantity = screenMenuItem.Quantity;
                SelectedMenuItems.Add(data);
                data.PublishEvent(EventTopicNames.ScreenMenuItemDataSelected);
            }
        }

        public void UpdateCurrentScreenMenu(int screenMenuId)
        {
            if (screenMenuId == 0) return;
            if (_currentScreenMenu != null && _currentScreenMenu.Id == screenMenuId) return;

            _currentScreenMenu = _cacheService.GetScreenMenu(screenMenuId);

            Categories = CreateCategoryButtons(_currentScreenMenu);
            MostUsedItemsCategory = null;
            MostUsedMenuItems = CreateMostUsedMenuItems(_currentScreenMenu);

            if (Categories != null && Categories.Count == 1)
            {
                OnCategoryCommandExecute(Categories[0].Category);
                Categories.Clear();
            }
            RaisePropertyChanged(() => Categories);
            RaisePropertyChanged(() => CategoriesVerticalAlignment);
            RaisePropertyChanged(() => MostUsedMenuItems);
            RaisePropertyChanged(() => MostUsedItemsCategory);
            RaisePropertyChanged(() => CategoryColumnCount);
            RaisePropertyChanged(() => CategoryColumnWidthRate);
        }

        private ObservableCollection<ScreenMenuItemButton> CreateMostUsedMenuItems(ScreenMenu screenMenu)
        {
            if (screenMenu != null)
            {
                MostUsedItemsCategory = screenMenu.Categories.FirstOrDefault(x => x.MostUsedItemsCategory);
                if (MostUsedItemsCategory != null)
                {
                    return new ObservableCollection<ScreenMenuItemButton>(
                    MostUsedItemsCategory.ScreenMenuItems.OrderBy(x => x.SortOrder).Select(x => new ScreenMenuItemButton(x, MenuItemCommand, MostUsedItemsCategory)));
                }
            }
            return null;
        }

        private void UpdateMenuButtons(ScreenMenuCategory category)
        {
            MenuItems = CreateMenuButtons(category, CurrentPageNo, CurrentTag ?? "");

            SubCategories.Clear();
            SubCategories.AddRange(
                category.GetScreenMenuCategories(CurrentTag)
                .Select(x => new ScreenSubCategoryButton(x, SubCategoryCommand, GetCategorySubButtonColor(x, category), category.MainFontSize, category.SubButtonHeight)));

            if (!string.IsNullOrEmpty(CurrentTag))
            {
                var backButton = new ScreenSubCategoryButton(CurrentTag.Replace(CurrentTag.Split(',').Last(), "").Trim(new[] { ',', ' ' }), SubCategoryCommand, "Gainsboro", category.MainFontSize, category.SubButtonHeight, true);
                SubCategories.Add(backButton);
            }

            if (Categories != null && MenuItems.Count == 0)
            {
                if (category.NumeratorType == 2 && SubCategories.Count == 0)
                    InteractionService.ShowKeyboard();

                MenuItems.Clear();

                if (category.MaxItems > 0)
                {
                    IEnumerable<ScreenMenuItem> sitems = category.ScreenMenuItems.OrderBy(x => x.SortOrder);
                    if (SubCategories.Count == 0)
                    {
                        sitems = Categories.Select(x => x.Category).SelectMany(x => x.ScreenMenuItems);
                    }
                    var items = sitems.Select(x => new ScreenMenuItemButton(x, MenuItemCommand, SelectedCategory));
                    MenuItems.AddRange(items.Take(category.MaxItems));
                }
            }

            RaisePropertyChanged(() => MenuItems);
            RaisePropertyChanged(() => IsPageNumberNavigatorVisible);
            RaisePropertyChanged(() => MenuItemsVerticalAlignment);
            RaisePropertyChanged(() => SubButtonRows);
        }

        private string GetCategorySubButtonColor(string name, ScreenMenuCategory category)
        {
            if (string.IsNullOrEmpty(category.SubButtonColorDef) || !category.SubButtonColorDef.Contains(name + "=")) return category.MainButtonColor;
            return category.SubButtonColorDef;
        }

        private void OnSubCategoryCommand(ScreenSubCategoryButton obj)
        {
            CurrentTag = obj.Name.Trim();
            UpdateMenuButtons(SelectedCategory);
        }

        private void OnCategoryCommandExecute(ScreenMenuCategory category)
        {
            CurrentPageNo = 0;
            CurrentTag = "";
            UpdateMenuButtons(category);
            if (IsQuickNumeratorVisible)
            {
                QuickNumeratorValues = string.IsNullOrEmpty(category.NumeratorValues) ? new[] { "1", "2", "3", "4", "5" } : category.NumeratorValues.Split(',');
                NumeratorValue = QuickNumeratorValues[0] != "1" ? QuickNumeratorValues[0] : "";
            }
            else NumeratorValue = "";

            AlphaButtonValues = string.IsNullOrEmpty(category.AlphaButtonValues) ? new string[0] : category.AlphaButtonValues.Split(',');

            RaisePropertyChanged(() => IsQuickNumeratorVisible);
            RaisePropertyChanged(() => IsNumeratorVisible);
            RaisePropertyChanged(() => IsNumberpadEditorVisible);
            RaisePropertyChanged(() => QuickNumeratorValues);
            RaisePropertyChanged(() => AlphaButtonValues);
            RaisePropertyChanged(() => MenuItemsVerticalAlignment);
            RaisePropertyChanged(() => IsSelectedItemsVisible);
        }

        private ObservableCollection<ScreenMenuItemButton> CreateMenuButtons(ScreenMenuCategory category, int pageNo, string tag)
        {
            SelectedCategory = category;

            var screenMenuItems = category.GetScreenMenuItems(pageNo, tag);
            var result = new ObservableCollection<ScreenMenuItemButton>();
            var items = screenMenuItems.Select(x => new ScreenMenuItemButton(x, MenuItemCommand, category));
            result.AddRange(items);
            return result;
        }

        private ObservableCollection<ScreenCategoryButton> CreateCategoryButtons(ScreenMenu screenMenu)
        {
            if (screenMenu != null)
            {
                if (MenuItems != null) MenuItems.Clear();

                _currentScreenMenu = screenMenu;
                var result = new ObservableCollection<ScreenCategoryButton>();

                foreach (var category in screenMenu.Categories.OrderBy(x => x.SortOrder).Where(x => !x.MostUsedItemsCategory))
                {
                    var sButton = new ScreenCategoryButton(category, CategoryCommand);
                    result.Add(sButton);
                }

                if (result.Count > 0)
                {
                    var c = result.First();
                    if (_selectedCategory != null)
                        c = result.SingleOrDefault(x => x.Category.Name.ToLower() == _selectedCategory.Name.ToLower());
                    if (c == null && result.Count > 0) c = result.ElementAt(0);
                    if (c != null) OnCategoryCommandExecute(c.Category);
                }

                return result;
            }

            Reset();

            return Categories;
        }

        public void Reset()
        {
            if (MenuItems != null) MenuItems.Clear();
            if (Categories != null) Categories.Clear();
            _currentScreenMenu = null;
        }

        private void OnTypeValueExecute(string obj)
        {
            if (obj == "\r")
            {
                if (_filtered && MenuItems.Count == 1)
                    MenuItemCommand.Execute(MenuItems[0].ScreenMenuItem);
                else
                    FindMenuItem();
            }
            else if (obj == "\b" && !string.IsNullOrEmpty(NumeratorValue))
                NumeratorValue = NumeratorValue.Substring(0, NumeratorValue.Length - 1);
            else if (!string.IsNullOrEmpty(obj) && !Char.IsControl(obj[0]))
                NumeratorValue = Utility.AddTypedValue(NumeratorValue, obj, "#0.");
        }

        public bool HandleTextInput(string text)
        {
            //if (IsNumeratorVisible)
            //{
            OnTypeValueExecute(text);
            return true;
            //}
            //return false;
        }
    }
}
