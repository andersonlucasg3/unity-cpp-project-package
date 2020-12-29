using System.Collections.Concurrent;
using System.Diagnostics;
using static UnityCpp.Editor.Utils.RegexUtils;

namespace UnityCpp.Editor.Builds
{
    internal class BuildOutputParser
    {
        internal readonly ConcurrentQueue<ProgressConfig> configs;

        public BuildOutputParser()
        {
            configs = new ConcurrentQueue<ProgressConfig>();
        }

        public void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
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
                        return MatchRegex(line, "/(\\w+)\\.cpp", fileName =>
                        {
                            config.info = $"Compiled {fileName}.cpp";
                            return true;
                        });
                    }
                    return true;
                });
                if (!fileRegexResult && !MatchRegex(line, ".+Built\\s\\w+\\s(\\w+)", buildModule =>
                {
                    config.progress = 1F;
                    config.info = $"Linked {buildModule}";
                    return true;
                }))
                {
                    return;
                }
                configs.Enqueue(config);
            }
        }

        
    }
}