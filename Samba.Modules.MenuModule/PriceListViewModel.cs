using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Persistance.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [Export(typeof(PriceListViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class PriceListViewModel : VisibleViewModelBase
    {
        private readonly IPriceListService _priceListService;

        [ImportingConstructor]
        public PriceListViewModel(IPriceListService priceListService)
        {
            _priceListService = priceListService;
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave);
        }

        public ICaptionCommand SaveCommand { get; set; }

        private ObservableCollection<PriceViewModel> _items;
        public ObservableCollection<PriceViewModel> Items
        {
            get { return _items ?? (_items = CreateItems()); }
        }

        private void OnSave(object obj)
        {
            _priceListService.UpdatePrices(Items.Where(x => x.IsChanged).Select(x => new PriceData(x.Model, x.ItemName)).ToList());
            foreach (var priceViewModel in Items)
            {
                priceViewModel.IsChanged = false;
            }
            _items = null;
            RaisePropertyChanged(() => Items);
        }

        public IEnumerable<string> PriceTags { get { return Items.SelectMany(x => x.Model.Prices.Select(y => y.GetTrimmedPriceTag())).Distinct(); } }

        private ObservableCollection<PriceViewModel> CreateItems()
        {
            var tags = _priceListService.GetTags();

            var result = new ObservableCollection<PriceViewModel>(
                _priceListService.CreatePrices().Select(x => new PriceViewModel(x.Portion, x.Name, tags)));

            foreach (var tag in tags)
            {
                var tagValue = tag;
                var portions = result.Where(x => !x.Model.Prices.Select(y => y.PriceTag).Contains(tagValue)).ToList();
                portions.ForEach(x => x.AddPrice(tagValue));
            }

            return result;
        }

        protected override string GetHeaderInfo()
        {
            return Resources.BatchPriceList;
        }

        public override Type GetViewType()
        {
            return typeof(PriceListView);
        }
    }
}
