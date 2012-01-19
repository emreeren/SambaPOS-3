using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ModelBase
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class EntityCollectionViewModelBase<TViewModel, TModel> : AbstractEntityCollectionViewModelBase, ICountable
        where TViewModel : EntityViewModelBase<TModel>
        where TModel : class, IEntity, new()
    {
        [ImportingConstructor]
        public EntityCollectionViewModelBase()
        {
            OpenViewModels = new List<EntityViewModelBase<TModel>>();
            BatchCreateItemsCommand = new CaptionCommand<TModel>(string.Format("Batch Create {0}", PluralModelTitle), OnBatchCreateItems, CanBatchCreateItems);

            CustomCommands.Add(BatchCreateItemsCommand);

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

        private static bool CanBatchCreateItems(TModel arg)
        {
            return typeof(TModel).GetInterfaces().Any(x => x == typeof(IBatchCreatable));
        }

        private void OnBatchCreateItems(TModel obj)
        {
            var title = string.Format("Batch Create {0}", ModelTitle);
            var description = string.Format("We'll create {0} for each line you entered here.", PluralModelTitle);
            var data = InteractionService.UserIntraction.GetStringFromUser(title, description);
            if (data.Length > 0)
            {
                foreach (var s in data)
                {
                    var b = Activator.CreateInstance<TModel>() as IBatchCreatable;
                    if (b != null) b.UpdatePropertiesFromString(s);
                    Workspace.Add(b as TModel);
                }
                Workspace.CommitChanges();
                _items = null;
                RaisePropertyChanged(() => Items);
            }
        }

        public ICaptionCommand BatchCreateItemsCommand { get; set; }

        private readonly IWorkspace _workspace = WorkspaceFactory.Create();
        public IWorkspace Workspace { get { return _workspace; } }

        private ObservableCollection<TViewModel> _items;
        public ObservableCollection<TViewModel> Items { get { return _items ?? (_items = GetItemsList()); } }

        protected virtual ObservableCollection<TViewModel> GetItemsList()
        {
            return BuildViewModelList(SelectItems());
        }

        protected virtual IEnumerable<TModel> SelectItems()
        {
            return _workspace.All<TModel>();
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
            VisibleViewModelBase wm = InternalCreateNewViewModel(model);
            if (wm is EntityViewModelBase<TModel>)
                OpenViewModels.Add(wm as EntityViewModelBase<TModel>);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override void OnDuplicateItem(object obj)
        {
            var duplicate = ObjectCloner.Clone(SelectedItem.Model);
            duplicate.Id = 0;
            duplicate.Name = "_" + duplicate.Name;
            VisibleViewModelBase wm = InternalCreateNewViewModel(duplicate);
            if (wm is EntityViewModelBase<TModel>)
                OpenViewModels.Add(wm as EntityViewModelBase<TModel>);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override void OnDeleteSelectedItems(IEnumerable obj)
        {
            obj.Cast<TViewModel>().ToList().ForEach(
                model =>
                {
                    if (model.Model.Id > 0)
                    {
                        BeforeDeleteItem(model.Model);
                        Workspace.Delete(model.Model);
                    }
                    Items.Remove(model);
                });
            Workspace.CommitChanges();
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

        public int GetCount()
        {
            return Items.Count;
        }
    }
}
