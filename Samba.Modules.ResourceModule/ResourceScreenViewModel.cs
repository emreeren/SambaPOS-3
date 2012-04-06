using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.ResourceModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ResourceScreenViewModel : EntityViewModelBase<ResourceScreen>
    {
        public ICaptionCommand SelectScreenItemsCommand { get; set; }

        public IEnumerable<ResourceScreenItem> ResourceScreenItems { get { return Model.ScreenItems; } }

        public string[] DisplayModes { get { return new[] { Resources.Automatic, Resources.Custom, Resources.Hidden }; } }
        public string DisplayMode { get { return DisplayModes[Model.DisplayMode]; } set { Model.DisplayMode = Array.IndexOf(DisplayModes, value); } }
        public string BackgroundImage { get { return string.IsNullOrEmpty(Model.BackgroundImage) ? "/Images/empty.png" : Model.BackgroundImage; } set { Model.BackgroundImage = value; } }
        public string BackgroundColor { get { return string.IsNullOrEmpty(Model.BackgroundColor) ? "Transparent" : Model.BackgroundColor; } set { Model.BackgroundColor = value; } }
        public string LocationEmptyColor { get { return Model.LocationEmptyColor; } set { Model.LocationEmptyColor = value; } }
        public string LocationFullColor { get { return Model.LocationFullColor; } set { Model.LocationFullColor = value; } }
        public string LocationLockedColor { get { return Model.LocationLockedColor; } set { Model.LocationLockedColor = value; } }
        public int PageCount { get { return Model.PageCount; } set { Model.PageCount = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ResourceTemplateId { get { return Model.ResourceTemplateId; } set { Model.ResourceTemplateId = value; } }

        public ResourceScreenViewModel()
        {
            SelectScreenItemsCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.ScreenItem), OnSelectScreenItems);
        }

        private IEnumerable<ResourceTemplate> _resourceTemplates;
        public IEnumerable<ResourceTemplate> ResourceTemplates
        {
            get { return _resourceTemplates ?? (_resourceTemplates = Workspace.All<ResourceTemplate>()); }
        }

        private void OnSelectScreenItems(string obj)
        {

            //if (SelectedCategory != null)
            //{
            //    IList<IOrderable> values = new List<IOrderable>(Workspace.All<MenuItem>().OrderBy(x => x.GroupCode + x.Name)
            //        .Where(x => !SelectedCategory.ContainsMenuItem(x))
            //        .Select(x => new ScreenMenuItem { MenuItemId = x.Id, Name = x.Name, MenuItem = x }));

            //    IList<IOrderable> selectedValues = new List<IOrderable>(SelectedCategory.ScreenMenuItems);

            //    var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, Resources.ProductList,
            //        string.Format(Resources.AddProductsToCategoryHint_f, SelectedCategory.Name), Resources.Product, Resources.Products);

            //    foreach (var screenMenuItem in SelectedCategory.ScreenMenuItems.ToList())
            //    {
            //        if (!choosenValues.Contains(screenMenuItem) && screenMenuItem.Id > 0)
            //            Workspace.Delete(screenMenuItem);
            //    }

            //    SelectedCategory.ScreenMenuItems.Clear();

            //    foreach (ScreenMenuItem item in choosenValues)
            //    {
            //        SelectedCategory.ScreenMenuItems.Add(item);
            //    }

            //    SelectedCategory.UpdateDisplay();
            //}

            IList<IOrderable> values = new List<IOrderable>(Workspace
                .All<Resource>(x => x.ResourceTemplateId == ResourceTemplateId)
                .Where(x => ResourceScreenItems.FirstOrDefault(y => y.ResourceId == x.Id) == null)
                .OrderBy(x => x.Name)
                .Select(x => new ResourceScreenItem { ResourceId = x.Id, Name = x.Name }));

            IList<IOrderable> selectedValues = new List<IOrderable>(ResourceScreenItems);
            IList<IOrderable> choosenValues =
                InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, string.Format(Resources.List_f, Resources.Resource),
                string.Format(Resources.SelectItemsFor_f, Resources.Resourceses, Model.Name, Resources.ResourceScreen), Resources.Resource, Resources.Resourceses);

            Model.ScreenItems.Clear();
            foreach (ResourceScreenItem choosenValue in choosenValues)
            {
                Model.AddScreenItem(choosenValue);
            }
            RaisePropertyChanged(() => ResourceScreenItems);
        }

        public override Type GetViewType()
        {
            return typeof(ResourceScreenView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ResourceScreen;
        }

        protected override AbstractValidator<ResourceScreen> GetValidator()
        {
            return new ResourceScreenValidator();
        }
    }

    internal class ResourceScreenValidator : EntityValidator<ResourceScreen>
    {
        public ResourceScreenValidator()
        {
            RuleFor(x => x.ResourceTemplateId).GreaterThan(0);
        }
    }

}
