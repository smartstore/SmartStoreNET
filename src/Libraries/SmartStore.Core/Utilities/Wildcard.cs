using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartStore.Utilities
{

    public class Wildcard
    {
        private readonly Regex _regex;
        //private readonly string _pattern;

        public Wildcard(string pattern)
            : this(pattern, RegexOptions.None)
        {
        }

        public Wildcard(string pattern, RegexOptions options)
        {
            Guard.ArgumentNotEmpty(() => pattern);
            _regex = new Regex(WildcardToRegex(pattern), options);
        }

        public bool IsMatch(string input)
        {
            Guard.ArgumentNotEmpty(() => input);
            return _regex.IsMatch(input);
        }

        public static bool IsMatch(string input, string pattern)
        {
            return IsMatch(input, pattern, RegexOptions.None);
        }

        public static bool IsMatch(string input, string pattern, RegexOptions options)
        {
            Guard.ArgumentNotNull(() => input);
            return new Wildcard(pattern, options).IsMatch(input);
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

        }

        public override string ToString()
        {
            return _regex.ToString();
        }

    }

}
