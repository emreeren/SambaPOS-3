using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    class MenuItemPriceDefinitionViewModel : EntityViewModelBase<MenuItemPriceDefinition>
    {
        public MenuItemPriceDefinitionViewModel(MenuItemPriceDefinition model)
            : base(model)
        {
        }

        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        public override Type GetViewType()
        {
            return typeof(MenuItemPriceDefinitionView);
        }

        public override string GetModelTypeString()
        {
            return Resources.PriceDefinition;
        }

        protected override string GetSaveErrorMessage()
        {
            if (Model.Id == 0 && Dao.Count<MenuItemPriceDefinition>(x => x.PriceTag.ToLower() == Model.PriceTag.ToLower()) > 0)
            {
                return string.Format(Resources.ThereIsAnotherPriceDefinition_f, Model.PriceTag);
            }
            var mip = Dao.Single<MenuItemPriceDefinition>(x => x.PriceTag == Model.PriceTag);
            return mip != null && mip.Id != Model.Id ? string.Format(Resources.ThereIsAnotherPriceDefinition_f, Model.PriceTag) : base.GetSaveErrorMessage();
        }

        protected override void OnSave(string value)
        {
            if (Model.Id > 0)
            {
                var mip = Dao.Single<MenuItemPriceDefinition>(x => x.Id == Model.Id);
                if (mip.PriceTag != Model.PriceTag)
                {
                    using (var workspace = WorkspaceFactory.Create())
                    {
                        workspace.All<MenuItemPrice>(x => x.PriceTag == mip.PriceTag)
                            .ToList()
                            .ForEach(x => x.PriceTag = Model.PriceTag);
                        workspace.CommitChanges();
                    }
                }
            }
            base.OnSave(value);
        }
    }
}
