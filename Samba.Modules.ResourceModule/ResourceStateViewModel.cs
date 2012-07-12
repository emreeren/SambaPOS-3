using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.ResourceModule
{
    class ResourceStateViewModel : EntityViewModelBase<ResourceState>
    {
        public string Color { get { return Model.Color; } set { Model.Color = value; } }
        
        public override Type GetViewType()
        {
            return typeof(ResourceStateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ResourceState;
        }
    }
}
