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
                var result = Resources.ResourceManager.GetString(_resourceName);
                if (string.IsNullOrEmpty(result))
                    result =
                        _resourceName.Select(x => char.IsLower(x) ? x.ToString() : string.Format(" {0}", x)).Aggregate((x, y) => x + y).Trim();
                return result;
            }
        }
    }
}
