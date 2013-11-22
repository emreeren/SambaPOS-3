using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.EntityModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class EntityScreenViewModel : EntityViewModelBaseWithMap<EntityScreen, EntityScreenMap, AbstractMapViewModel<EntityScreenMap>>
    {
        private readonly IEntityDao _entityDao;

        [ImportingConstructor]
        public EntityScreenViewModel(IEntityDao entityDao)
        {
            _entityDao = entityDao;
            SelectScreenItemsCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.Entity), OnSelectScreenItems, CanSelectScreenItems);
        }

        public ICaptionCommand SelectScreenItemsCommand { get; set; }

        public string[] DisplayModes { get { return new[] { Resources.Automatic, Resources.Search, Resources.Custom }; } }
        public string DisplayMode { get { return DisplayModes[Model.DisplayMode]; } set { Model.DisplayMode = Array.IndexOf(DisplayModes, value); } }
        public string BackgroundImage { get { return string.IsNullOrEmpty(Model.BackgroundImage) ? "/Images/empty.png" : Model.BackgroundImage; } set { Model.BackgroundImage = value; } }
        public string BackgroundColor { get { return string.IsNullOrEmpty(Model.BackgroundColor) ? "Transparent" : Model.BackgroundColor; } set { Model.BackgroundColor = value; } }
        public int FontSize { get { return Model.FontSize; } set { Model.FontSize = value; } }
        public int PageCount { get { return Model.PageCount; } set { Model.PageCount = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int RowCount { get { return Model.RowCount; } set { Model.RowCount = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int TicketTypeId { get { return Model.TicketTypeId; } set { Model.TicketTypeId = value; } }
        public int? EntityTypeId { get { return Model.EntityTypeId; } set { Model.EntityTypeId = value.GetValueOrDefault(0); } }
        public string StateFilter { get { return Model.StateFilter; } set { Model.StateFilter = value; } }
        public string DisplayState { get { return Model.DisplayState; } set { Model.DisplayState = value; } }
        public bool AskTicketType { get { return Model.AskTicketType; } set { Model.AskTicketType = value; } }
        public string SearchValueReplacePattern { get { return Model.SearchValueReplacePattern; } set { Model.SearchValueReplacePattern = value; } }

        private IEnumerable<TicketType> _ticketTypes;
        public IEnumerable<TicketType> TicketTypes
        {
            get { return _ticketTypes ?? (_ticketTypes = Workspace.All<TicketType>()); }
        }

        private IEnumerable<EntityScreenItem> _entityScreenItems;
        public IEnumerable<EntityScreenItem> EntityScreenItems
        {
            get { return _entityScreenItems ?? (_entityScreenItems = new List<EntityScreenItem>(Model.ScreenItems)); }
        }

        private IEnumerable<EntityType> _entityTypes;
        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes ?? (_entityTypes = Workspace.All<EntityType>()); }
        }

        private IEnumerable<State> _entityStates;
        public IEnumerable<State> EntityStates
        {
            get { return _entityStates ?? (_entityStates = Workspace.All<State>()); }
        }

        private bool CanSelectScreenItems(string arg)
        {
            return Model.EntityTypeId > 0;
        }

        private void OnSelectScreenItems(string obj)
        {
            var entityType = _entityDao.GetEntityTypeById(EntityTypeId.GetValueOrDefault(0));
            var items = Model.ScreenItems.ToList();

            IList<IOrderable> values = new List<IOrderable>(Workspace
                .All<Entity>(x => x.EntityTypeId == EntityTypeId)
                .Where(x => items.FirstOrDefault(y => y.EntityId == x.Id) == null)
                .OrderBy(x => x.Name)
                .Select(x => new EntityScreenItem(entityType, x)));

            IList<IOrderable> selectedValues = new List<IOrderable>(items);
            IList<IOrderable> choosenValues =
                InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, string.Format(Resources.List_f, Resources.Entity),
                string.Format(Resources.SelectItemsFor_f, Resources.Entities, Model.Name, Resources.EntityScreen), Resources.Entity, Resources.Entities);

            Model.ScreenItems.Clear();
            foreach (EntityScreenItem choosenValue in choosenValues)
            {
                Model.AddScreenItem(choosenValue);
            }
            _entityScreenItems = null;
            RaisePropertyChanged(() => EntityScreenItems);
        }

        public override Type GetViewType()
        {
            return typeof(EntityScreenView);
        }

        public override string GetModelTypeString()
        {
            return Resources.EntityScreen;
        }

        protected override AbstractValidator<EntityScreen> GetValidator()
        {
            return new EntityScreenValidator();
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<EntityScreenMap, AbstractMapViewModel<EntityScreenMap>>(Model.EntityScreenMaps, Workspace);
        }
    }

    internal class EntityScreenValidator : EntityValidator<EntityScreen>
    {
        public EntityScreenValidator()
        {
            RuleFor(x => x.TicketTypeId).GreaterThan(0);
            RuleFor(x => x.EntityTypeId).GreaterThan(0).When(x => x.DisplayMode < 2);
        }
    }
}
