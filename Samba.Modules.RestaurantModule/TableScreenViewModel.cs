using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Tables;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.RestaurantModule
{
    public class TableScreenViewModel : EntityViewModelBase<TableScreen>
    {
        public ICaptionCommand SelectTablesCommand { get; set; }
        private ObservableCollection<TableScreenItemViewModel> _screenItems;
        public ObservableCollection<TableScreenItemViewModel> ScreenItems
        {
            get { return _screenItems ?? (_screenItems = new ObservableCollection<TableScreenItemViewModel>(Model.Tables.Select(x => new TableScreenItemViewModel(x, Model)))); }
        }

        public string[] DisplayModes { get { return new[] { Resources.Automatic, Resources.Custom, Resources.Hidden }; } }
        public string DisplayMode { get { return DisplayModes[Model.DisplayMode]; } set { Model.DisplayMode = Array.IndexOf(DisplayModes, value); } }
        public string BackgroundImage { get { return string.IsNullOrEmpty(Model.BackgroundImage) ? "/Images/empty.png" : Model.BackgroundImage; } set { Model.BackgroundImage = value; } }
        public string BackgroundColor { get { return string.IsNullOrEmpty(Model.BackgroundColor) ? "Transparent" : Model.BackgroundColor; } set { Model.BackgroundColor = value; } }
        public string TableEmptyColor { get { return Model.TableEmptyColor; } set { Model.TableEmptyColor = value; } }
        public string TableFullColor { get { return Model.TableFullColor; } set { Model.TableFullColor = value; } }
        public string TableLockedColor { get { return Model.TableLockedColor; } set { Model.TableLockedColor = value; } }
        public int PageCount { get { return Model.PageCount; } set { Model.PageCount = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int NumeratorHeight { get { return Model.NumeratorHeight; } set { Model.NumeratorHeight = value; } }
        public string AlphaButtonValues { get { return Model.AlphaButtonValues; } set { Model.AlphaButtonValues = value; } }

        public TableScreenViewModel(TableScreen model)
            : base(model)
        {
            SelectTablesCommand = new CaptionCommand<string>(Resources.SelectTable, OnSelectTables);
        }

        private void OnSelectTables(string obj)
        {
            IList<IOrderable> values = new List<IOrderable>(Workspace.All<Table>()
                .Where(x => ScreenItems.SingleOrDefault(y => y.Model.Id == x.Id) == null));

            IList<IOrderable> selectedValues = new List<IOrderable>(ScreenItems.Select(x => x.Model));

            IList<IOrderable> choosenValues =
                InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, Resources.TableList,
                string.Format(Resources.SelectTableDialogHint_f, Model.Name), Resources.Table, Resources.Tables);

            ScreenItems.Clear();
            Model.Tables.Clear();

            foreach (Table choosenValue in choosenValues)
            {
                Model.AddScreenItem(choosenValue);
                ScreenItems.Add(new TableScreenItemViewModel(choosenValue, Model));
            }
        }

        public override Type GetViewType()
        {
            return typeof(TableScreenView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TableView;
        }
    }
}
