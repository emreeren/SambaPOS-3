namespace Samba.Domain.Models.Automation
{
    public static class Operations
    {
        public const string IsNull = "IsNull";
        public const string IsNotNull = "IsNotNull";
        public const string Equal = "Equal";
        public const string NotEquals = "NotEquals";
        public const string Greater = "Greater";
        public const string Less = "Less";
        public const string Contains = "Contains";
        public const string Starts = "Starts";
        public const string Ends = "Ends";
        public const string LengthEquals = "LengthEquals";
        public const string Matches = "Matches";
        public const string NotMatches = "NotMatches";
        public const string MatchesMod10 = "MatchesMod10";
        public const string After = "After";
        public const string Before = "Before";

        public static string[] AllOperations = new[] { Equal, NotEquals, Greater, Less, IsNull, IsNotNull, Starts, Ends, Contains, LengthEquals, Matches, NotMatches, MatchesMod10, After, Before };
        public static string[] StringOperations = new[] { Equal, NotEquals, IsNull, IsNotNull, Starts, Ends, Contains, LengthEquals, Matches, NotMatches, MatchesMod10, After, Before };
        public static string[] NumericOperations = new[] { Equal, NotEquals, Greater, Less };

    }
}