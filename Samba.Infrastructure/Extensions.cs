using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Samba.Infrastructure
{
    public static class Extensions
    {
        public static dynamic ToDynamic(this object value)
        {
            if (value is ExpandoObject) return value;
            IDictionary<string, object> expando = new ExpandoObject();

            if(value != null)
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }
    }
}
