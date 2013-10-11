using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Data.Validation;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common.ModelBase
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class EntityCollectionViewModelBase<TViewModel, TModel> : AbstractEntityCollectionViewModelBase, ICountable
        where TViewModel : EntityViewModelBase<TModel>
        where TModel : class, IEntityClass, new()
    {
        [ImportingConstructor]
        public EntityCollectionViewModelBase()
        {
            Limit = LocalSettings.DefaultRecordLimit;
            OpenViewModels = new List<EntityViewModelBase<TModel>>();
            BatchCreateItemsCommand = new CaptionCommand<TModel>(string.Format(Resources.BatchCreate_f, PluralModelTitle), OnBatchCreateItems, CanBatchCreateItems);
            SortItemsCommand = new CaptionCommand<TModel>(string.Format(Resources.Sort_f, PluralModelTitle), OnSortItems);
            RemoveLimitCommand = new CaptionCommand<TModel>(Resources.RemoveLimit, OnRemoveLimit);
            ToggleOrderByIdCommand = new CaptionCommand<TModel>(Resources.ChangeSortOrder, OnToggleOrderById);

            if (typeof(TViewModel).GetInterfaces().Any(x => x == typeof(IEntityCreator<TModel>)))
                CustomCommands.Add(BatchCreateItemsCommand);

            CustomCommands.Add(typeof(TModel).GetInterfaces().Any(x => x == typeof(IOrderable))
                                   ? SortItemsCommand
                                   : ToggleOrderByIdCommand);

            _token = EventServiceFactory.EventService.GetEvent<GenericEvent<EntityViewModelBase<TModel>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.AddedModelSaved)
                    if (x.Value is TViewModel)
                        Items.Add(x.Value as TViewModel);

                if (x.Topic == EventTopicNames.ModelAddedOrDeleted)
                {
                    foreach (var openViewModel in OpenViewModels)
                    {
                        if (!openViewModel.CanSave())
                            openViewModel.RollbackModel();
                    }

                    if (x.Value is TViewModel)
                    {
                        Dao.RemoveFromCache(x.Value.Model as ICacheable);
                        _workspace.Update(x.Value.Model);
                        _workspace.CommitChanges();
                        _workspace.Refresh(x.Value.Model);
                    }
                }
            });

            _token2 = EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Subscribe(
                  s =>
                  {
                      if (s.Topic == EventTopicNames.ViewClosed)
                      {
                          if (s.Value is EntityViewModelBase<TModel> && OpenViewModels.Contains(s.Value))
                              OpenViewModels.Remove(s.Value as EntityViewModelBase<TModel>);
                      }
                  });
        }

        private void OnToggleOrderById(TModel obj)
        {
            ToggleOrderBy();
        }

        private void OnRemoveLimit(TModel obj)
        {
            Limit = 0;
            _items = null;
            RaisePropertyChanged(() => Items);
            RaisePropertyChanged(() => DisplayLimitWarning);
        }

        private void OnSortItems(TModel obj)
        {
            var list = Items.Select(x => x.Model).ToList();
            InteractionService.UserIntraction.SortItems(list.Cast<IOrderable>(), string.Format(Resources.Sort_f, PluralModelTitle), Resources.ChangeSortOrderHint);
            Workspace.CommitChanges();
            _items = null;
            RaisePropertyChanged(() => Items);
        }

        private static bool CanBatchCreateItems(TModel arg)
        {
            return typeof(TViewModel).GetInterfaces().Any(x => x == typeof(IEntityCreator<TModel>));
        }

        private void OnBatchCreateItems(TModel obj)
        {
            var title = string.Format(Resources.BatchCreate_f, PluralModelTitle);
            var description = string.Format(Resources.BatchCreateInfo_f, PluralModelTitle);
            var data = InteractionService.UserIntraction.GetStringFromUser(title, description);
            if (data.Length > 0)
            {
                var items = ((IEntityCreator<TModel>)InternalCreateNewViewModel(new TModel())).CreateItems(data);
                foreach (var item in items) Workspace.Add(item);
                Workspace.CommitChanges();
                _items = null;
                RaisePropertyChanged(() => Items);
            }
        }

        public ICaptionCommand BatchCreateItemsCommand { get; set; }
        public ICaptionCommand SortItemsCommand { get; set; }
        public ICaptionCommand RemoveLimitCommand { get; set; }
        public ICaptionCommand ToggleOrderByIdCommand { get; set; }

        public bool DisplayLimitWarning
        {
            get
            {
                return Limit > 0 && _items != null && _items.Count == Limit;
            }
        }

        private readonly IWorkspace _workspace = WorkspaceFactory.Create();
        public IWorkspace Workspace { get { return _workspace; } }

        private ObservableCollection<TViewModel> _items;
        public ObservableCollection<TViewModel> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = GetItemsList();
                    RaisePropertyChanged(() => DisplayLimitWarning);
                }
                return _items;
            }
        }

        public string Filter { get; set; }
        public int Limit { get; set; }
        public bool OrderByDescending
        {
            get { return EntityCollectionSortManager.GetOrderByDesc<TModel>(); }
            set { EntityCollectionSortManager.SetOrderByDesc<TModel>(value); }
        }

        public override void RefreshItems()
        {
            _items = null;
            RaisePropertyChanged(() => Items);
        }

        protected virtual ObservableCollection<TViewModel> GetItemsList()
        {
            return BuildViewModelList(SelectItems());
        }

        protected virtual IEnumerable<TModel> SelectItems()
        {
            var filter = (Filter ?? "").ToLower();
            return !string.IsNullOrEmpty(filter)
                ? _workspace.Query<TModel>(x => x.Name.ToLower().Contains(filter), Limit, OrderByDescending)
                : _workspace.Query<TModel>(Limit, OrderByDescending);
        }

        protected virtual void BeforeDeleteItem(TModel item)
        {
            // override if needed.
        }

        private void DoDeleteItem(TModel item)
        {
            BeforeDeleteItem(item);
            _workspace.Delete(item);
            _workspace.CommitChanges();
        }

        private readonly SubscriptionToken _token;
        private readonly SubscriptionToken _token2;
        public IList<EntityViewModelBase<TModel>> OpenViewModels { get; set; }

        private TViewModel _selectedItem;
        public TViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);
            }
        }

        public override string GetModelTitle()
        {
            return CreateNewViewModel(new TModel()).GetModelTypeString();
        }

        protected virtual string CanDeleteItem(TModel model)
        {
            return ValidatorRegistry.GetDeleteErrorMessage(model);
        }

        protected override bool CanAddItem(object obj)
        {
            return true;
        }

        protected override bool CanClose(object arg)
        {
            return OpenViewModels.Count == 0;
        }

        protected override void OnDeleteItem(object obj)
        {
            if (MessageBox.Show(string.Format(Resources.DeleteItemConfirmation_f, ModelTitle, SelectedItem.Model.Name), Resources.Confirmation, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var errorMessage = CanDeleteItem(SelectedItem.Model);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (SelectedItem.Model.Id > 0)
                    {
                        DoDeleteItem(SelectedItem.Model);
                        SelectedItem.Model.PublishEvent(EventTopicNames.ModelAddedOrDeleted);
                    }
                    Items.Remove(SelectedItem);
                }
                else
                {
                    MessageBox.Show(errorMessage, Resources.Warning);
                }
            }
        }

        protected override void OnAddItem(object obj)
        {
            var model = new TModel();
            PublishViewModel(model);
        }

        public void PublishViewModel(TModel model)
        {
            VisibleViewModelBase wm = InternalCreateNewViewModel(model);
            if (wm != null) OpenViewModels.Add(wm as EntityViewModelBase<TModel>);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override void OnDuplicateItem(object obj)
        {
            var duplicate = ObjectCloner.EntityClone(SelectedItem.Model);
            duplicate.Id = 0;
            EntityIdFixer.FixEntityIdNumber(duplicate, x => 0);
            duplicate.Name = "_" + duplicate.Name;
            VisibleViewModelBase wm = InternalCreateNewViewModel(duplicate);
            if (wm != null) OpenViewModels.Add(wm as EntityViewModelBase<TModel>);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override void OnDeleteSelectedItems(IEnumerable obj)
        {
            var errors = new StringBuilder();

            if (MessageBox.Show(Resources.DeleteSelectedItems + "?", Resources.Confirmation, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            obj.Cast<TViewModel>().ToList().ForEach(
                model =>
                {
                    var errorMessage = CanDeleteItem(model.Model);
                    if (!string.IsNullOrEmpty(errorMessage))
                        errors.AppendLine(errorMessage);
                    else
                    {
                        if (model.Model.Id > 0)
                        {
                            BeforeDeleteItem(model.Model);
                            Workspace.Delete(model.Model);
                        }
                        Items.Remove(model);
                    }
                });
            Workspace.CommitChanges();
            if (!string.IsNullOrEmpty(errors.ToString()))
                MessageBox.Show(errors.ToString());
        }

        protected override bool CanDeleteSelectedItems(IEnumerable arg)
        {
            return SelectedItem != null;
        }

        protected override bool CanDuplicateItem(object arg)
        {
            return SelectedItem != null;
        }

        protected override bool CanEditItem(object obj)
        {
            return SelectedItem != null;
        }

        protected override bool CanDeleteItem(object obj)
        {
            return SelectedItem != null && !OpenViewModels.Contains(SelectedItem);
        }

        protected override void OnEditItem(object obj)
        {
            if (!OpenViewModels.Contains(SelectedItem)) OpenViewModels.Add(SelectedItem);
            (SelectedItem as VisibleViewModelBase).PublishEvent(EventTopicNames.ViewAdded);
        }

        protected ObservableCollection<TViewModel> BuildViewModelList(IEnumerable<TModel> itemsList)
        {
            if (typeof(TModel).GetInterfaces().Any(x => x == typeof(IOrderable)))
                return new ObservableCollection<TViewModel>(itemsList.OrderBy(x => ((IOrderable)x).SortOrder).ThenBy(x => x.Id).Select(InternalCreateNewViewModel));
            return new ObservableCollection<TViewModel>(itemsList.Select(InternalCreateNewViewModel));
        }

        protected TViewModel CreateNewViewModel(TModel model)
        {
            TViewModel result;
            try
            {
                result = ServiceLocator.Current.GetInstance<TViewModel>();
            }
            catch (Exception)
            {
                result = Activator.CreateInstance<TViewModel>();
            }
            result.Model = model;
            return result;
        }

        protected TViewModel InternalCreateNewViewModel(TModel model)
        {
            var result = CreateNewViewModel(model);
            result.Init(_workspace, model);
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_workspace != null)
                    _workspace.Dispose();
                EventServiceFactory.EventService.GetEvent<GenericEvent<EntityViewModelBase<TModel>>>().Unsubscribe(_token);
                EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Unsubscribe(_token2);
            }
        }

        private void ToggleOrderBy()
        {
            OrderByDescending = !OrderByDescending;
            _items = null;
            RaisePropertyChanged(() => Items);
        }

        public int GetCount()
        {
            return Items.Count;
        }
    }
}
