using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartStore
{
    public static class RegexExtensions
    {
        public static string ReplaceGroup(this Regex regex, string input, string groupName, string replacement)
        {
            return ReplaceGroupInternal(regex, input, replacement, m => m.Groups[groupName]);
        }

        public static string ReplaceGroup(this Regex regex, string input, int groupNum, string replacement)
        {
            return ReplaceGroupInternal(regex, input, replacement, m => m.Groups[groupNum]);
        }

        private static string ReplaceGroupInternal(this Regex regex, string input, string replacement, Func<Match, Group> groupGetter)
        {
            return regex.Replace(input, match =>
            {
                var group = groupGetter(match);
                var sb = new StringBuilder();
                var previousCaptureEnd = 0;

                foreach (var capture in group.Captures.Cast<Capture>())
                {
                    var currentCaptureEnd = capture.Index + capture.Length - match.Index;
                    var currentCaptureLength = capture.Index - match.Index - previousCaptureEnd;

                    sb.Append(match.Value.Substring(previousCaptureEnd, currentCaptureLength));
                    sb.Append(replacement);

                    previousCaptureEnd = currentCaptureEnd;
                }

                sb.Append(match.Value.Substring(previousCaptureEnd));

                return sb.ToString();
            });
        }
    }
}
