using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UnityCpp.Editor.Builds
{
    internal class BuildOutputParser
    {
        internal readonly ConcurrentQueue<ProgressConfig> configs;

        public BuildOutputParser()
        {
            configs = new ConcurrentQueue<ProgressConfig>();
        }

        public virtual void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (sender)
            {
                string line = e.Data;
                
                if (string.IsNullOrEmpty(line)) return;

                ProgressConfig config = new ProgressConfig();

                bool fileRegexResult = MatchRegex(line, "\\[\\s+([0-9]+)%\\]", progress =>
                {
                    config.progress = float.Parse(progress) * .01F;
                    if (config.progress < 1F)
                    {
                        return MatchRegex(line, "/(\\w+\\.cpp)\\.o", fileName =>
                        {
                            config.info = $"Compiled {fileName}";
                            return true;
                        });
                    }
                    return true;
                });
                if (!fileRegexResult && !MatchRegex(line, ".+Built\\s\\w+\\s(\\w+)", buildModule =>
                {
                    config.progress = 1F;
                    config.info = $"Built module {buildModule}";
                    return true;
                }))
                {
                    return;
                }
                configs.Enqueue(config);
            }
        }

        protected static bool MatchRegex(string contents, string regex, Func<string, bool> matchAction)
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