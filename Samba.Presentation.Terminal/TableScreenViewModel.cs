using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tables;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Presentation.Terminal
{
    public delegate void TableSelectionEventHandler(Table selectedTable);

    public class TableScreenViewModel : ObservableObject
    {
        public int CurrentPageNo { get; set; }

        public ObservableCollection<TableScreenItemViewModel> Tables { get; set; }
        public TableScreen SelectedTableScreen { get { return AppServices.MainDataContext.SelectedTableScreen; } }

        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }
        public DelegateCommand<Table> SelectTableCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> FindTableCommand { get; set; }

        public event TableSelectionEventHandler TableSelectedEvent;

        public VerticalAlignment TableScreenAlignment { get { return SelectedTableScreen != null && SelectedTableScreen.ButtonHeight > 0 ? VerticalAlignment.Top : VerticalAlignment.Stretch; } }

        private bool _fullScreenNumerator;

        public void DisplayFullScreenNumerator()
        {
            _fullScreenNumerator = true;
            RaisePropertyChanged(() => NavigatorHeight);
            RaisePropertyChanged(() => IsTablesVisible);
            NumeratorHeight = double.NaN;
        }

        public void HideFullScreenNumerator()
        {
            NumeratorValue = "";
            _fullScreenNumerator = false;
            if (SelectedTableScreen != null)
                NumeratorHeight = SelectedTableScreen.NumeratorHeight;
            RaisePropertyChanged(() => NavigatorHeight);
            RaisePropertyChanged(() => NumeratorHeight);
            RaisePropertyChanged(() => IsTablesVisible);
        }

        public bool IsTablesVisible { get { return !_fullScreenNumerator; } }

        private double _numeratorHeight;
        public double NumeratorHeight
        {
            get { return _numeratorHeight; }
            set
            {
                _numeratorHeight = value;
                RaisePropertyChanged(() => NumeratorHeight);
            }
        }

        private string[] _alphaButtonValues;
        public string[] AlphaButtonValues
        {
            get { return _alphaButtonValues; }
            set
            {
                _alphaButtonValues = value;
                RaisePropertyChanged(() => AlphaButtonValues);
            }
        }

        private string _numeratorValue;
        public string NumeratorValue
        {
            get { return _numeratorValue; }
            set
            {
                _numeratorValue = value;
                RaisePropertyChanged(() => NumeratorValue);
            }
        }

        public TableScreenViewModel()
        {
            Tables = new ObservableCollection<TableScreenItemViewModel>();
            IncPageNumberCommand = new CaptionCommand<string>(Resources.NextPage, OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>(Resources.PreviousPage, OnDecPageNumber, CanDecPageNumber);
            SelectTableCommand = new DelegateCommand<Table>(OnSelectTable);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecute);
            FindTableCommand = new DelegateCommand<string>(OnFindTableExecute);
        }

        public void InvokeOnTableSelected(Table table)
        {
            var handler = TableSelectedEvent;
            if (handler != null) handler(table);
        }

        private void OnFindTableExecute(string obj)
        {
            if (!string.IsNullOrEmpty(NumeratorValue))
            {
                var table = AppServices.DataAccessService.GetTable(NumeratorValue);
                if (table != null)
                {
                    InvokeOnTableSelected(table);
                }
            }
            HideFullScreenNumerator();

        }

        private void OnTypeValueExecute(string obj)
        {
            if (obj == "\r")
                FindTableCommand.Execute("");
            else if (obj == "\b" && !string.IsNullOrEmpty(NumeratorValue))
                NumeratorValue = NumeratorValue.Substring(0, NumeratorValue.Length - 1);
            else
                NumeratorValue = obj == "clear" ? "" : Helpers.AddTypedValue(NumeratorValue, obj, "#0.");
        }

        private void OnSelectTable(Table obj)
        {
            InvokeOnTableSelected(obj);
        }

        public string NavigatorHeight
        {
            get { return SelectedTableScreen != null && !_fullScreenNumerator && SelectedTableScreen.PageCount > 1 ? "25" : "0"; }
        }

        private bool CanDecPageNumber(string arg)
        {
            return CurrentPageNo > 0;
        }

        private void OnDecPageNumber(string obj)
        {
            CurrentPageNo--;
            UpdateTables();
        }

        private bool CanIncPageNumber(string arg)
        {
            return SelectedTableScreen != null && CurrentPageNo < SelectedTableScreen.PageCount - 1;
        }

        private void OnIncPageNumber(string obj)
        {
            CurrentPageNo++;
            UpdateTables();
        }

        public void Refresh()
        {
            UpdateTables();
        }

        private void UpdateTables()
        {
            Tables.Clear();
            Tables.AddRange(AppServices.DataAccessService.GetCurrentTables(AppServices.MainDataContext.SelectedDepartment.TerminalTableScreens.FirstOrDefault(), CurrentPageNo)
                .Select(x => new TableScreenItemViewModel(x, AppServices.MainDataContext.SelectedTableScreen)));
            if (SelectedTableScreen != null)
            {
                AlphaButtonValues = !string.IsNullOrEmpty(SelectedTableScreen.AlphaButtonValues) ? SelectedTableScreen.AlphaButtonValues.Split(',') : new string[0];
                NumeratorHeight = SelectedTableScreen.NumeratorHeight;
            }
            else
            {
                NumeratorHeight = 0;
                AlphaButtonValues = new string[0];
            }
            RaisePropertyChanged(() => Tables);
            RaisePropertyChanged(() => NavigatorHeight);
            RaisePropertyChanged(() => SelectedTableScreen);
            RaisePropertyChanged(() => NumeratorHeight);
            RaisePropertyChanged(() => TableScreenAlignment);
        }
    }
}
