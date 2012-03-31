using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

        public IEnumerable<ResourceScreenItem> AccountScreenItems { get { return Model.ScreenItems; } }

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
        public int NumeratorHeight { get { return Model.NumeratorHeight; } set { Model.NumeratorHeight = value; } }
        public string AlphaButtonValues { get { return Model.AlphaButtonValues; } set { Model.AlphaButtonValues = value; } }

        public ResourceScreenViewModel()
        {
            SelectScreenItemsCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.ScreenItem), OnSelectScreenItems);
        }

        private void OnSelectScreenItems(string obj)
        {
            IList<IOrderable> values = new List<IOrderable>(Workspace.All<ResourceScreenItem>());
            IList<IOrderable> selectedValues = new List<IOrderable>(AccountScreenItems);
            IList<IOrderable> choosenValues =
                InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, string.Format(Resources.List_f, Resources.Location),
                string.Format(Resources.SelectLocationDialogHint_f, Model.Name), Resources.Location, Resources.Locations);

            Model.ScreenItems.Clear();
            foreach (ResourceScreenItem choosenValue in choosenValues)
            {
                Model.AddScreenItem(choosenValue);
            }
            RaisePropertyChanged(() => AccountScreenItems);
        }

        public override Type GetViewType()
        {
            return typeof(ResourceScreenView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ResourceScreen;
        }
    }
}
