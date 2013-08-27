namespace Samba.Domain.Models.Automation
{
    public static class Operations
    {
        public const string IsNull = "IsNull";
        public const string Equal = "Equal";
        public const string NotEquals = "NotEquals";
        public const string Greater = "Greater";
        public const string Less = "Less";
        public const string Contains = "Contains";
        public const string Starts = "Starts";
        public const string Ends = "Ends";
        public const string Matches = "Matches";
        public const string NotMatches = "NotMatches";

        public static string[] StringOperations = new[] { Equal, NotEquals, IsNull, Starts, Ends, Contains, Matches, NotMatches };
        public static string[] NumericOperations = new[] { Equal, NotEquals, Greater, Less };

    }
}