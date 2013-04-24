namespace Samba.Persistance.Common
{
    public class BalanceValue
    {
        private static BalanceValue _empty;
        public decimal Balance { get; set; }
        public decimal Exchange { get; set; }

        public static BalanceValue Empty
        {
            get
            {
                return _empty ?? (_empty = new BalanceValue());
            }
        }
    }
}