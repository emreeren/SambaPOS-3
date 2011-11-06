using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;

namespace Samba.Localization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string _resourceName;
        public LocalizedDisplayNameAttribute(string resourceName)
        {
            _resourceName = resourceName;
        }

        public LocalizedDisplayNameAttribute()
        {
            var type = GetType();
            _resourceName = type.Name;
        }

        public override string DisplayName
        {
            get
            {
                return Resources.ResourceManager.GetString(_resourceName);
            }
        }
    }
}
