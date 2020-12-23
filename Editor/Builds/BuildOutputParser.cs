using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UnityCpp.Editor.Builds
{
    internal abstract class BuildOutputParser
    {
        public readonly ConcurrentQueue<ProgressConfig> configs;

        public BuildOutputParser()
        {
            configs = new ConcurrentQueue<ProgressConfig>();
        }

        public abstract void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e);

        protected bool MatchRegex(string contents, string regex, Func<string, bool> matchAction)
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