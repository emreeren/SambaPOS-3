using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;

namespace Samba.Services.Implementations.ExpressionModule.Accessors
{
    public static class TicketAccessor
    {
        private static Ticket _model;
        public static Ticket Model
        {
            get { return _model ?? (_model = Ticket.Empty); }
            set { _model = value; }
        }

        public static int Id { get { return Model.Id; } }
        public static int OrderCount { get { return Model.Orders.Count; } }
        public static decimal RemainingAmount { get { return Model.GetRemainingAmount(); } }
        public static void RemovePayments(List<object> items)
        {
            items.Where(x => x is Payment).Cast<Payment>().ToList().ForEach(x => Model.Payments.Remove(x));
        }
        public static string CreatingUserName { get { return Model.Orders.Any() ? Model.Orders[0].CreatingUserName : ""; } }
        public static string LastUserName { get { return Model.Orders.Any() ? Model.Orders.OrderBy(x => x.Id).Last().CreatingUserName : ""; } }
        public static bool IsInState(string state)
        {
            if (state.Contains(":"))
            {
                var parts = state.Split(new[] { ':' }, 2);
                return InState(parts[0], parts[1]);
            }
            return Model.IsInState("*", state);
        }

        public static bool InState(string stateName, string state)
        {
            return Model.IsInState(stateName, state);
        }

        public static string GetState(string stateName)
        {
            return Model.GetStateStr(stateName);
        }

        public static decimal Sum { get { return Model.GetSum(); } }
        public static string SumF { get { return Sum.ToString(LocalSettings.CurrencyFormat); } }
        public static decimal Due { get { return RemainingAmount; } }
        public static string DueF { get { return Due.ToString(LocalSettings.CurrencyFormat); } }
        public static decimal SSum(string state) { return Model.GetOrderStateTotal(state); }
        public static string SSumF(string state) { return SSum(state).ToString(LocalSettings.CurrencyFormat); }

        public static string GetCustomData(string fieldName)
        {
            return _model.GetEntityFieldValue(fieldName);
        }
    }
}