using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Presentation.Common.ModelBase
{
    public class MapController<TModel, TViewModel>
        where TModel : class, IAbstractMapModel, new()
        where TViewModel : AbstractMapViewModel<TModel>, new()
    {
        private readonly IList<TModel> _itemsSource;
        private readonly IWorkspace _workspace;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IDepartmentService _departmentService;
        private readonly ICacheService _cacheService;
        
        private ObservableCollection<TViewModel> _maps;
        
        public CaptionCommand<string> DeleteMapCommand { get; set; }
        public CaptionCommand<string> AddMapCommand { get; set; }

        public MapController(IList<TModel> itemsSource, IWorkspace workspace)
        {
            _itemsSource = itemsSource;
            _workspace = workspace;
            _userService = ServiceLocator.Current.GetInstance<IUserService>();
            _settingService = ServiceLocator.Current.GetInstance<ISettingService>();
            _departmentService = ServiceLocator.Current.GetInstance<IDepartmentService>();
            _cacheService = ServiceLocator.Current.GetInstance<ICacheService>();

            AddMapCommand = new CaptionCommand<string>(Resources.Add, OnAddMap);
            DeleteMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteMap, CanDeleteMap);
        }

        private void OnDeleteMap(string obj)
        {
            if (SelectedMap.Id > 0)
                _workspace.Delete(SelectedMap.Model);
            _itemsSource.Remove(SelectedMap.Model);
            Maps.Remove(SelectedMap);
        }

        private void OnAddMap(string obj)
        {
            var map = new TModel();
            map.Initialize();
            _itemsSource.Add(map);
            Maps.Add(CreateNewViewModel(map));
        }

        private bool CanDeleteMap(string arg)
        {
            return SelectedMap != null;
        }

        public TViewModel SelectedMap { get; set; }

        public ObservableCollection<TViewModel> Maps { get { return _maps ?? (_maps = new ObservableCollection<TViewModel>(_itemsSource.Select(CreateNewViewModel))); } }

        private TViewModel CreateNewViewModel(TModel model)
        {
            return new TViewModel
                             {
                                 Model = model,
                                 UserService = _userService,
                                 SettingService = _settingService,
                                 DepartmentService = _departmentService,
                                 CacheService = _cacheService
                             };
        }

    }
}
