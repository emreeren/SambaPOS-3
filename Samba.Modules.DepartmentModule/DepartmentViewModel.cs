using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.DepartmentModule
{
    public class DepartmentViewModel : EntityViewModelBase<Department>
    {
        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = Dao.Query<ScreenMenu>()); }
            set { _screenMenus = value; }
        }

        private IEnumerable<TableScreen> _tableScreens;
        public IEnumerable<TableScreen> TableScreens
        {
            get { return _tableScreens ?? (_tableScreens = Dao.Query<TableScreen>()); }
            set { _tableScreens = value; }
        }

        private ObservableCollection<TableScreen> _posTableScreens;
        public ObservableCollection<TableScreen> PosTableScreens
        {
            get { return _posTableScreens ?? (_posTableScreens = new ObservableCollection<TableScreen>(Model.PosTableScreens.OrderBy(x => x.Order))); }
        }

        public int ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value; } }

        private IEnumerable<TicketTemplate> _ticketTemplates;
        public IEnumerable<TicketTemplate> TicketTemplates
        {
            get { return _ticketTemplates ?? (_ticketTemplates = Workspace.All<TicketTemplate>()); }
        }
        public TicketTemplate TicketTemplate { get { return Model.TicketTemplate; } set { Model.TicketTemplate = value; } }

        public int OpenTicketViewColumnCount { get { return Model.OpenTicketViewColumnCount; } set { Model.OpenTicketViewColumnCount = value; } }

        public bool IsFastFood
        {
            get { return Model.IsFastFood; }
            set { Model.IsFastFood = value; }
        }

        public bool IsAlaCarte
        {
            get { return Model.IsAlaCarte; }
            set { Model.IsAlaCarte = value; }
        }

        public bool IsTakeAway
        {
            get { return Model.IsTakeAway; }
            set { Model.IsTakeAway = value; }
        }

        public IEnumerable<string> PriceTags { get { return Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }
        
        public TableScreen SelectedPosTableScreen { get; set; }
        
        public ICaptionCommand AddPosTableScreenCommand { get; set; }
        public ICaptionCommand DeletePosTableScreenCommand { get; set; }

        public DepartmentViewModel(Department model)
            : base(model)
        {
            AddPosTableScreenCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.TableScreen), OnAddPosTableScreen);
            DeletePosTableScreenCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.TableScreen), OnDeletePosTableScreen, CanDeletePosTableScreen);
        }

        private bool CanDeletePosTableScreen(string arg)
        {
            return SelectedPosTableScreen != null;
        }

        private void OnDeletePosTableScreen(string obj)
        {
            Model.PosTableScreens.Remove(SelectedPosTableScreen);
            PosTableScreens.Remove(SelectedPosTableScreen);
        }

        private void OnAddPosTableScreen(string obj)
        {
            var selectedValues =
                  InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<TableScreen>().ToList<IOrderable>(),
                  Model.PosTableScreens.ToList<IOrderable>(), Resources.TableScreens, string.Format(Resources.ChooseServicesForDepartmentHint_f, Model.Name),
                  Resources.TableScreen, Resources.TableScreens);

            foreach (TableScreen selectedValue in selectedValues)
            {
                if (!Model.PosTableScreens.Contains(selectedValue))
                    Model.PosTableScreens.Add(selectedValue);
            }

            _posTableScreens = new ObservableCollection<TableScreen>(Model.PosTableScreens.OrderBy(x => x.Order));

            RaisePropertyChanged(() => PosTableScreens);
        }

        public override Type GetViewType()
        {
            return typeof(DepartmentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Department;
        }

        protected override AbstractValidator<Department> GetValidator()
        {
            return new DepartmentValidator();
        }
    }

    internal class DepartmentValidator : EntityValidator<Department>
    {
        public DepartmentValidator()
        {
            RuleFor(x => x.TicketTemplate).NotNull();
        }
    }
}
