using System;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    class ProductTimerViewModel : EntityViewModelBaseWithMap<ProductTimer, ProdcutTimerMap, ProductTimerMapViewModel>
    {
        private readonly string[] _priceTypes = new[] { Resources.Minute, Resources.Hour, Resources.Day };
        public string[] PriceTypes { get { return _priceTypes; } }

        public decimal PriceDuration { get { return Model.PriceDuration; } set { Model.PriceDuration = value; } }
        public string PriceType { get { return PriceTypes[Model.PriceType]; } set { Model.PriceType = PriceTypes.ToList().IndexOf(value); } }
        public decimal MinTime { get { return Model.MinTime; } set { Model.MinTime = value; } }
        public decimal TimeRounding { get { return Model.TimeRounding; } set { Model.TimeRounding = value; } }
        public int StartTime { get { return Model.StartTime; } set { Model.StartTime = value; } }

        public override Type GetViewType()
        {
            return typeof(ProductTimerView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ProductTimer;
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<ProdcutTimerMap, ProductTimerMapViewModel>(Model.ProductTimerMaps, Workspace);
        }
    }
}
