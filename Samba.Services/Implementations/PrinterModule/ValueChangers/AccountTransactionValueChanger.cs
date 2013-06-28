using System.ComponentModel.Composition;
using Samba.Domain.Models.Accounts;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    [Export]
    public class AccountTransactionValueChanger : AbstractValueChanger<AccountTransaction>
    {
        public override string GetTargetTag()
        {
            return "TRANSACTIONS";
        }

        protected override string GetModelName(AccountTransaction model)
        {
            return model.Name;
        }
    }
}