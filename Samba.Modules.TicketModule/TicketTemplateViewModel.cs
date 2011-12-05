using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    class TicketTemplateViewModel : EntityViewModelBase<TicketTemplate>
    {
        public TicketTemplateViewModel(TicketTemplate model) : base(model)
        {
        }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators { get { return _numerators ?? (_numerators = Workspace.All<Numerator>()); } set { _numerators = value; } }

        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }


        public override Type GetViewType()
        {
            return typeof (TicketTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TicketTemplate;
        }

        protected override AbstractValidator<TicketTemplate> GetValidator()
        {
            return new TicketTemplateValidator();
        }
    }

    internal class TicketTemplateValidator : EntityValidator<TicketTemplate>
    {
        public TicketTemplateValidator()
        {
            RuleFor(x => x.TicketNumerator).NotNull();
            RuleFor(x => x.OrderNumerator).NotNull();
        }
    }
}
