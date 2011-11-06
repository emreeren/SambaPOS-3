using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class PrintJobListViewModel : EntityCollectionViewModelBase<PrintJobViewModel, PrintJob>
    {
        protected override PrintJobViewModel CreateNewViewModel(PrintJob model)
        {
            return new PrintJobViewModel(model);
        }

        protected override PrintJob CreateNewModel()
        {
            return new PrintJob();
        }
    }
}
