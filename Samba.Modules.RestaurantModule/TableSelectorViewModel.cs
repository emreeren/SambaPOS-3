using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.RestaurantModule
{
    [Export]
    public class TableSelectorViewModel : ObservableObject
    {
        public DelegateCommand<TableScreenItemViewModel> TableSelectionCommand { get; set; }
        public DelegateCommand<TableScreen> SelectTableCategoryCommand { get; set; }
        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand EditSelectedTableScreenPropertiesCommand { get; set; }
        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }

        public ObservableCollection<IDiagram> Tables { get; set; }

        public Ticket SelectedTicket { get { return AppServices.MainDataContext.SelectedTicket; } }
        public TableScreen SelectedTableScreen { get { return AppServices.MainDataContext.SelectedTableScreen; } }
        public IEnumerable<TableScreen> TableScreens { get { return AppServices.MainDataContext.SelectedDepartment != null ? AppServices.MainDataContext.SelectedDepartment.PosTableScreens : null; } }

        public bool IsNavigated { get; set; }
        public bool CanDesignTables { get { return AppServices.CurrentLoggedInUser.UserRole.IsAdmin; } }
        public int CurrentPageNo { get; set; }

        public bool IsPageNavigatorVisible { get { return SelectedTableScreen != null && SelectedTableScreen.PageCount > 1; } }
        public bool IsFeedbackVisible { get { return !string.IsNullOrEmpty(Feedback); } }
        private string _feedback;
        public string Feedback
        {
            get { return _feedback; }
            set
            {
                _feedback = value;
                RaisePropertyChanged(() => Feedback);
                RaisePropertyChanged(() => IsFeedbackVisible);
            }
        }

        private string _feedbackColor;
        public string FeedbackColor
        {
            get { return _feedbackColor; }
            set { _feedbackColor = value; RaisePropertyChanged(() => FeedbackColor); }
        }

        private string _feedbackForeground;
        public string FeedbackForeground
        {
            get { return _feedbackForeground; }
            set
            {
                _feedbackForeground = value;
                RaisePropertyChanged(() => FeedbackForeground);
            }
        }

        public VerticalAlignment TablesVerticalAlignment { get { return SelectedTableScreen != null && SelectedTableScreen.ButtonHeight > 0 ? VerticalAlignment.Top : VerticalAlignment.Stretch; } }

        public TableSelectorViewModel()
        {
            SelectTableCategoryCommand = new DelegateCommand<TableScreen>(OnSelectTableCategoryExecuted);
            TableSelectionCommand = new DelegateCommand<TableScreenItemViewModel>(OnSelectTableExecuted);
            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreenExecuted);
            EditSelectedTableScreenPropertiesCommand = new CaptionCommand<string>(Resources.Properties, OnEditSelectedTableScreenProperties, CanEditSelectedTableScreenProperties);
            IncPageNumberCommand = new CaptionCommand<string>(Resources.NextPage + " >>", OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>("<< " + Resources.PreviousPage, OnDecPageNumber, CanDecPageNumber);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectTable)
                    {
                        RefreshTables();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
                x =>
                {
                    if (AppServices.ActiveAppScreen == AppScreens.TableList
                        && x.Topic == EventTopicNames.MessageReceivedEvent
                        && x.Value.Command == Messages.TicketRefreshMessage)
                    {
                        RefreshTables();
                    }
                });
        }

        public void RefreshTables()
        {
            if (SelectedTableScreen == null && TableScreens.Count() > 0)
                AppServices.MainDataContext.SelectedTableScreen = TableScreens.First();
            if (SelectedTableScreen != null)
                UpdateTables(SelectedTableScreen);
        }

        private bool CanDecPageNumber(string arg)
        {
            return SelectedTableScreen != null && CurrentPageNo > 0;
        }

        private void OnDecPageNumber(string obj)
        {
            CurrentPageNo--;
            RefreshTables();
        }

        private bool CanIncPageNumber(string arg)
        {
            return SelectedTableScreen != null && CurrentPageNo < SelectedTableScreen.PageCount - 1;
        }

        private void OnIncPageNumber(string obj)
        {
            CurrentPageNo++;
            RefreshTables();
        }

        private bool CanEditSelectedTableScreenProperties(string arg)
        {
            return SelectedTableScreen != null;
        }

        private void OnEditSelectedTableScreenProperties(string obj)
        {
            if (SelectedTableScreen != null)
                InteractionService.UserIntraction.EditProperties(SelectedTableScreen);
        }

        private void OnCloseScreenExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(IsNavigated
                                                              ? EventTopicNames.ActivateNavigation
                                                              : EventTopicNames.DisplayTicketView);
        }

        private void OnSelectTableCategoryExecuted(TableScreen obj)
        {
            UpdateTables(obj);
        }

        private static void OnSelectTableExecuted(TableScreenItemViewModel obj)
        {
            var location = new LocationData
                               {
                                   LocationId = obj.Model.Id,
                                   LocationName = obj.Model.Name,
                                   TicketId = obj.Model.TicketId,
                                   Caption = obj.Caption
                               };
            location.PublishEvent(EventTopicNames.LocationSelectedForTicket);
        }

        private void UpdateTables(TableScreen tableScreen)
        {
            Feedback = "";
            var tableData = AppServices.DataAccessService.GetCurrentTables(tableScreen, CurrentPageNo).OrderBy(x => x.Order);
            if (Tables != null && (Tables.Count() == 0 || Tables.Count != tableData.Count() || Tables.First().Caption != tableData.First().Name)) Tables = null;
            if (Tables == null)
            {
                Tables = new ObservableCollection<IDiagram>();
                Tables.AddRange(tableData.Select(x => new TableScreenItemViewModel(x, SelectedTableScreen, TableSelectionCommand)));
            }
            else
            {
                for (var i = 0; i < tableData.Count(); i++)
                {
                    ((TableScreenItemViewModel)Tables[i]).Model = tableData.ElementAt(i);
                }
            }

            if (SelectedTicket != null && !string.IsNullOrEmpty(SelectedTicket.LocationName))
            {
                FeedbackColor = "Red";
                FeedbackForeground = "White";
                Feedback = string.Format(Resources.SelectTableThatYouWantToMoveTicket_f, SelectedTicket.LocationName);
            }
            else if (SelectedTicket != null)
            {
                FeedbackColor = "Red";
                FeedbackForeground = "White";
                Feedback = Resources.SelectTableForTicket;
            }
            else
            {
                FeedbackColor = "LightYellow";
                FeedbackForeground = "Black";
                Feedback = Resources.SelectTableForOperation;
            }

            RaisePropertyChanged(() => Tables);
            RaisePropertyChanged(() => TableScreens);
            RaisePropertyChanged(() => SelectedTableScreen);
            RaisePropertyChanged(() => IsPageNavigatorVisible);
            RaisePropertyChanged(() => TablesVerticalAlignment);
        }

        public void LoadTrackableTables()
        {
            Tables = new ObservableCollection<IDiagram>(
                AppServices.MainDataContext.LoadTables(SelectedTableScreen.Name)
                .Select<Table, IDiagram>(x => new TableScreenItemViewModel(x, SelectedTableScreen)));
            RaisePropertyChanged(() => Tables);
        }

        public void SaveTrackableTables()
        {
            AppServices.MainDataContext.SaveTables();
            UpdateTables(SelectedTableScreen);
        }
    }
}
