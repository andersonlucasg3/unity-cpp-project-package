#if UNITY_EDITOR_OSX
using System.Diagnostics;

namespace UnityCpp.Editor.Builds
{
    internal class GccOutputParser : BuildOutputParser
    {
        public override void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
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
    }
}
#endif