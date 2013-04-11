using System;
using System.Collections.Generic;
using Fluentscript.Lib._Core;
using Fluentscript.Lib._Core.Meta.Docs;
using Fluentscript.Lib._Core.Meta.Types;
// <lang:using>

// </lang:using>


namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class for processing doctags.
    /// </summary>
    public class DocHelper
    {
        /// <summary>
        /// Parses doctags supporting simple format for the 1st release.
        /// </summary>
        /// <param name="comments"></param>
        /// <returns></returns>
        public static Tuple<DocTags, List<string>> ParseDocTags(List<string> comments)
        {
            var commentTokens = new List<Token>();
            foreach (var comment in comments)
                commentTokens.Add(TokenBuilder.ToComment(false, comment));

            return ParseDocTags(commentTokens);
        }


        /// <summary>
        /// Parses doctags supporting simple format for the 1st release.
        /// </summary>
        /// <param name="comments"></param>
        /// <returns></returns>
        public static Tuple<DocTags, List<string>> ParseDocTags(List<Token> comments)
        {
            var tags = new DocTags();
            var warnings = new List<string>();

            // What was the last comment type
            var lastType = "";
            foreach (var c in comments)
            {
                var comment = c.Text;
                if (comment.StartsWith("#"))
                    comment = comment.Substring(1);

                comment = comment.Trim();

                if (comment.Contains("@summary"))
                {
                    Try(() => tags.Summary = comment.Replace("@summary:", "").Trim(), "summary", comment, 1, warnings);
                    lastType = "sum";
                }
                else if (comment.Contains("@arg"))
                {
                    Try(() => tags.Args.Add(ParseArg(comment)), "arg", comment, 1, warnings);
                    lastType = "arg";
                }
                else if (comment.Contains("@example"))
                {
                    Try(() => tags.Examples.Add(ParseExample(comment)), "example", comment, 1, warnings);
                    lastType = "ex";
                }
                else if(comment.StartsWith("@"))
                {
                    Try(() =>
                            {
                                var tag = ParseCustomTag(comment);
                                tags.CustomTags.Add(tag);
                                lastType = tag.Name;
                            }, "custom tag", comment, 1, warnings);
                }
                else if(lastType != "sum" && string.IsNullOrEmpty(comment))
                {
                    continue;
                }
                else
                {
                    if (lastType == "sum")
                        tags.Summary += " " + comment;
                }
            }
            return new Tuple<DocTags, List<string>>(tags, warnings);
        }


        /// <summary>
        /// Parses an arg attribute
        /// </summary>
        /// <param name="tagLine">Parses a custom tag.</param>
        /// <returns></returns>
        private static CustomTag ParseCustomTag(string tagLine)
        {
            if (string.IsNullOrEmpty(tagLine))
                return new CustomTag();

            tagLine = tagLine.Trim();
            var ndxColon = tagLine.IndexOf(':');
            var tag = new CustomTag();
            tag.Name = tagLine.Substring(1, ndxColon - 1);
            tag.Content = tagLine.Substring(ndxColon + 1);
            tag.Content = tag.Content.Trim();
            return tag;
        }


        /// <summary>
        /// Parses an arg attribute
        /// </summary>
        /// <param name="argText">@arg: date,   The date to buy the stock,   date  , on,     July 10th 2012 | 7/10/2012</param>
        /// <returns></returns>
        private static ArgAttribute ParseArg(string argText)
        {
            if (string.IsNullOrEmpty(argText))
                return new ArgAttribute();

            argText = StripArgDocTag(argText);

            // Named values.
            if (argText.Contains(":"))
                return ParseArgByNamedProperties(argText);

            return ParseArgByPositionProperties(argText);
        }


        /// <summary>
        /// Parses arguments by 
        /// </summary>
        /// <param name="argText"></param>
        /// <returns></returns>
        private static ArgAttribute ParseArgByNamedProperties(string argText)
        {
            // Remove the @arg: from the argText            
            var arg = new ArgAttribute();
            // NOTE: Do a simple split on "," for the 1st version of fluentscript.
            string[] fields = argText.Split(',');
            int totalFields = fields.Length;
            foreach (var field in fields)
            {
                var tokens = field.Split(':');

                // Name 
                string name = tokens[0].Trim().ToLower();
                string value = tokens[1].Trim();
                if (name == "name") arg.Name = value;
                else if (name == "desc") arg.Desc = value;
                else if (name == "alias") arg.Alias = value;
                else if (name == "type") arg.Type = value;
                else if (name == "examples")
                {
                    ParseExamples(arg, value);
                }
            }
            return arg;
        }


        /// <summary>
        /// Parses arguments by 
        /// </summary>
        /// <param name="argText"></param>
        /// <returns></returns>
        private static ArgAttribute ParseArgByPositionProperties(string argText)
        {
            var arg = new ArgAttribute();
            // NOTE: Do a simple split on "," for the 1st version of fluentscript.
            string[] fields = argText.Split(',');
            int totalFields = fields.Length;

            // Get the name.
            if (totalFields >= 1) arg.Name = fields[0].Trim();
            if (totalFields >= 2) arg.Desc = fields[1].Trim();
            if (totalFields >= 3) arg.Type = fields[2].Trim();
            if (totalFields >= 4) arg.Alias = fields[3].Trim();

            if (totalFields >= 5)
            {
                var example = fields[4];
                ParseExamples(arg, example);
            }
            return arg;
        }


        private static string StripArgDocTag(string argText)
        {
            argText = argText.Replace("@arg", "");
            argText = argText.Trim();
            if (argText[0] == ':')
                argText = argText.Substring(1);
            return argText;
        }


        /// <summary>
        /// Parses an arg attribute
        /// </summary>
        /// <param name="exText">@example: conventional sytax, 'orderToBuy( shares(300), "IBM", 40.5, new Date(2012, 7, 10)', "premium policy"'</param>
        /// <returns></returns>
        private static Example ParseExample(string exText)
        {
            var example = new Example();
            if (string.IsNullOrEmpty(exText))
                return example;

            // NOTE: Do a simple split on "," for the 1st version of fluentscript.
            int ndxFirstComma = exText.IndexOf(",");
            int ndxFirstQuote = exText.IndexOf("'");
            int ndxLastQuote  = exText.LastIndexOf("'");
            example.Desc = exText.Substring(9, ndxFirstComma - 9).Trim();
            example.Code = exText.Substring(ndxFirstQuote + 1, (ndxLastQuote - ndxFirstQuote) -1).Trim();
            return example;
        }


        private static void ParseExamples(ArgAttribute arg, string example)
        {
            if (!example.Contains("|"))
                arg.Examples.Add(example);
            else
            {
                string[] examples = example.Split('|');
                foreach (var ex in examples)
                    arg.Examples.Add(ex.Trim());
            }
        }


        private static void Try(Action action, string errorType, string line, int lineNum, List<string> warnings)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                warnings.Add("Warning:  comment for " + errorType + " is not valid at line number : " + lineNum + ". " + ex.Message);
            }
        }
    }
}
