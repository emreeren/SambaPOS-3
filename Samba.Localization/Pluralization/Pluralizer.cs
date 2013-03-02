using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;

namespace Samba.Localization.Pluralization
{
    public static class Pluralizer
    {
        static Pluralizer()
        {
            Rules.Add("en", new EnglishPluralizationRule());
            Rules.Add("tr", new TurkishPluralizationRule());
            Rules.Add("*", new DefaultPluralizationrule());
        }

        private static readonly Dictionary<string, PluralizationRule> Rules = new Dictionary<string, PluralizationRule>();

        private static PluralizationRule GetPluralizationRule()
        {
            if (Rules.ContainsKey(LocalSettings.CurrentLanguage))
                return Rules[LocalSettings.CurrentLanguage];
            return Rules["*"];

        }

        public static string ToPlural(string singular)
        {
            return GetPluralizationRule().Pluralize(singular);
        }
    }

    public abstract class PluralizationRule
    {
        public abstract string Pluralize(string singular);
    }

    public class EnglishPluralizationRule : PluralizationRule
    {
        private static readonly IList<string> Unpluralizables = new List<string> { "equipment", "information", "rice", "money", "species", "series", "fish", "sheep", "deer" };
        private static readonly IDictionary<string, string> Pluralizations = new Dictionary<string, string>
                                                                                 {
                                                                                     { "person$", "people" },
                                                                                     { "ox$", "oxen" },
                                                                                     { "child$", "children" },
                                                                                     { "foot$", "feet" },
                                                                                     { "tooth$", "teeth" },
                                                                                     { "goose$", "geese" },
                                                                                     { "(.*)fe?", "$1ves" },         
                                                                                     { "(.*)man$", "$1men" },
                                                                                     { "(.+[aeiou]y)$", "$1s" },
                                                                                     { "(.+[^aeiou])y$", "$1ies" },
                                                                                     { "(.+z)$", "$1zes" },
                                                                                     { "([m|l])ouse$", "$1ice" },
                                                                                     { "(.+)(e|i)x$", @"$1ices"},
                                                                                     { "(octop|vir)us$", "$1i"},
                                                                                     { "(.+(s|x|sh|ch))$", @"$1es"},
                                                                                     { "(.+)", @"$1s" }
                                                                                 };

        public override string Pluralize(string singular)
        {
            if (Unpluralizables.Contains(singular))
                return singular;
            var plural = "";
            foreach (var pluralization in Pluralizations.Where(pluralization => Regex.IsMatch(singular, pluralization.Key)))
            {
                plural = Regex.Replace(singular, pluralization.Key, pluralization.Value);
                break;
            }
            return plural;
        }
    }

    public class TurkishPluralizationRule : PluralizationRule
    {
        private static readonly IDictionary<string, string> Pluralizations = new Dictionary<string, string>
                                                                                 {
                                                                                     { "(?=^[\\w]+$)(.+[eiöü].?)$", "$1ler" },
                                                                                     { "(?=^[\\w]+$)(.+[aıou].?)$", "$1lar" },
                                                                                     { "(.+)s?[eiöü]$", "$1leri" },
                                                                                     { "(.+)s?[aıou]$", "$1ları" }
                                                                                 };

        public override string Pluralize(string singular)
        {
            foreach (var pluralization in Pluralizations.Where(pluralization => Regex.IsMatch(singular, pluralization.Key)))
            {
                return Regex.Replace(singular, pluralization.Key, pluralization.Value);
            }

            //var ses = new[] { 'a', 'e', 'ı', 'i', 'o', 'ö', 'u', 'ü' };
            //var inceSes = new[] { 'e', 'i', 'ö', 'ü' };
            //bool suffixed = singular.Contains(" ") && ses.Contains(singular[singular.Length - 1]);
            //singular = suffixed ? singular.Remove(singular.Length - 1) : singular;
            //var lastSes = singular.ToCharArray().Where(ses.Contains).Last();
            //var ek = inceSes.Contains(lastSes) ? (suffixed ? "leri" : "ler") : suffixed ? "ları" : "lar";
            //return singular + ek;
            return singular;
        }
    }

    public class DefaultPluralizationrule : PluralizationRule
    {
        public override string Pluralize(string singular)
        {
            return String.Format(Resources.List_f, singular);
        }
    }
}
