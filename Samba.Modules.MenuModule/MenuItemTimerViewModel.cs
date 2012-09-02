using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    class MenuItemTimerViewModel : EntityViewModelBaseWithMap<MenuItemTimer, MenuItemTimerMap, MenuItemTimerMapViewModel>
    {
        private readonly string[] _priceTypes = new[] { Resources.Minute, Resources.Hour, Resources.Day };
        public string[] PriceTypes { get { return _priceTypes; } }

        public decimal PriceDuration { get { return Model.PriceDuration; } set { Model.PriceDuration = value; } }
        public string PriceType { get { return PriceTypes[Model.PriceType]; } set { Model.PriceType = PriceTypes.ToList().IndexOf(value); } }
        public decimal MinTime { get { return Model.MinTime; } set { Model.MinTime = value; } }
        public decimal TimeRounding { get { return Model.TimeRounding; } set { Model.TimeRounding = value; } }

        public override Type GetViewType()
        {
            return typeof(MenuItemTimerView);
        }

        public override string GetModelTypeString()
        {
            return Resources.MenuItemTimer;
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<MenuItemTimerMap, MenuItemTimerMapViewModel>(Model.MenuItemTimerMaps, Workspace);
        }
    }
}
