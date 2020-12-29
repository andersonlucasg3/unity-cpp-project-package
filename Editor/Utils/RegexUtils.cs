using System;
using System.Text.RegularExpressions;

namespace UnityCpp.Editor.Utils
{
    internal static class RegexUtils
    {
        public static bool MatchRegex(string contents, string regex, Func<string, bool> matchAction)
        {
            Match match = Regex.Match(contents, regex);
            if (!match.Success) return false;

            Group group = match.Groups[1];
            if (!group.Success) return false;

            Capture capture = @group.Captures[0];

            return matchAction.Invoke(capture.Value);
        }
    }
}