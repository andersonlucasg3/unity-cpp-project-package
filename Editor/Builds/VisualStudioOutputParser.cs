#if UNITY_EDITOR_WIN
using System.Diagnostics;

namespace UnityCpp.Editor.Builds
{
    internal class VisualStudioOutputParser : BuildOutputParser
    {
        private float _overallProgress = 0F;
        
        public override void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (sender)
            {
                string line = e.Data;
                
                if (string.IsNullOrEmpty(line)) return;

                bool regexResult = MatchRegex(line, "(\\w+)\\.cpp", match =>
                {
                    configs.Enqueue(new ProgressConfig
                    {
                        progress = _overallProgress += 0.001F,
                        info = $"Compiling {match}.cpp"
                    });
                    return true;
                });
                if (!regexResult)
                {
                    MatchRegex(line, "\\\\(\\w+)\\.dll", match =>
                    {
                        configs.Enqueue(new ProgressConfig
                        {
                            progress = _overallProgress += 0.025F,
                            info = $"Finished compiling {match}.dll"
                        });
                        return true;
                    });
                }
            }
        }
    }
}

#endif