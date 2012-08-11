using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    interface IValueChanger<in T>
    {
        string Replace(PrinterTemplate template, string content, IEnumerable<T> models);
        string GetValue(PrinterTemplate template, T model);
    }
}
