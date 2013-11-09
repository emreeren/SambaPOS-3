using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Validation;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.Implementations
{
    [Export(typeof(IAutomationDao))]
    class AutomationDao : IAutomationDao
    {
        [ImportingConstructor]
        public AutomationDao()
        {
            ValidatorRegistry.RegisterDeleteValidator<AppAction>(x => Dao.Exists<ActionContainer>(y => y.AppActionId == x.Id), Resources.Action, Resources.Rule);
            ValidatorRegistry.RegisterSaveValidator(new AppActionSaveValidator());
        }

        public Dictionary<string, string> GetScripts()
        {
            return Dao.Query<Script>().ToDictionary(x => x.HandlerName, x => x.Code);
        }

        public AppAction GetActionById(int appActionId)
        {
            return Dao.Single<AppAction>(x => x.Id == appActionId);
        }

        public IEnumerable<string> GetAutomationCommandNames()
        {
            return Dao.Distinct<AutomationCommand>(x => x.Name);
        }
    }

    internal class AppActionSaveValidator : SpecificationValidator<AppAction>
    {
        public override string GetErrorMessage(AppAction model)
        {
            if (model.Parameters.Values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Any(x => x.EndsWith(" ") || x.StartsWith(" ")))
            {
                var pair = model.Parameters.First(x => x.Value.EndsWith(" ") || x.Value.StartsWith(" "));
                return string.Format(Resources.ParameterNameContainsSpaceError_f, pair.Key);
            }
            return "";
        }
    }
}
