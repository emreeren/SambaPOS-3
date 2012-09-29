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

        private IEnumerable<ResourceScreenItem> _resourceScreenItems;
        public IEnumerable<ResourceScreenItem> ResourceScreenItems
        {
            get { return _resourceScreenItems ?? (_resourceScreenItems = new List<ResourceScreenItem>(Model.ScreenItems)); }
        }

        public string[] DisplayModes { get { return new[] { Resources.Automatic, Resources.Custom, Resources.Search }; } }
        public string DisplayMode { get { return DisplayModes[Model.DisplayMode]; } set { Model.DisplayMode = Array.IndexOf(DisplayModes, value); } }
        public string BackgroundImage { get { return string.IsNullOrEmpty(Model.BackgroundImage) ? "/Images/empty.png" : Model.BackgroundImage; } set { Model.BackgroundImage = value; } }
        public string BackgroundColor { get { return string.IsNullOrEmpty(Model.BackgroundColor) ? "Transparent" : Model.BackgroundColor; } set { Model.BackgroundColor = value; } }
        public int PageCount { get { return Model.PageCount; } set { Model.PageCount = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int RowCount { get { return Model.RowCount; } set { Model.RowCount = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ResourceTypeId { get { return Model.ResourceTypeId; } set { Model.ResourceTypeId = value; } }
        public int? StateFilterId { get { return Model.StateFilterId; } set { Model.StateFilterId = value.GetValueOrDefault(0); } }

        public ResourceScreenViewModel()
        {
            SelectScreenItemsCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.ScreenItem), OnSelectScreenItems);
        }

        private IEnumerable<ResourceType> _ResourceTypes;
        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _ResourceTypes ?? (_ResourceTypes = Workspace.All<ResourceType>()); }
        }

        private IEnumerable<ResourceState> _resourceStates;
        public IEnumerable<ResourceState> ResourceStates
        {
            get { return _resourceStates ?? (_resourceStates = Workspace.All<ResourceState>()); }
        }

        private void OnSelectScreenItems(string obj)
        {
            var items = Model.ScreenItems.ToList();

            IList<IOrderable> values = new List<IOrderable>(Workspace
                .All<Resource>(x => x.ResourceTypeId == ResourceTypeId)
                .Where(x => items.FirstOrDefault(y => y.ResourceId == x.Id) == null)
                .OrderBy(x => x.Name)
                .Select(x => new ResourceScreenItem { ResourceId = x.Id, Name = x.Name }));

            IList<IOrderable> selectedValues = new List<IOrderable>(items);
            IList<IOrderable> choosenValues =
                InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, string.Format(Resources.List_f, Resources.Resource),
                string.Format(Resources.SelectItemsFor_f, Resources.Resourceses, Model.Name, Resources.ResourceScreen), Resources.Resource, Resources.Resourceses);

            Model.ScreenItems.Clear();
            foreach (ResourceScreenItem choosenValue in choosenValues)
            {
                Model.AddScreenItem(choosenValue);
            }
            _resourceScreenItems = null;
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
            RuleFor(x => x.ResourceTypeId).GreaterThan(0);
        }
    }
}
