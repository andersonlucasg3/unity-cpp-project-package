using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UnityCpp.Editor.Builds
{
    internal class WindowsBuildOutputParser : BuildOutputParser
    {
        private readonly float _fixedPercentage = default;

        public WindowsBuildOutputParser(float fixedPercentage)
        {
            _fixedPercentage = fixedPercentage;
        }
        
        public override void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (sender)
            {
                string line = e.Data;

                if (string.IsNullOrEmpty(line)) return;
                
                Debug.Log(line);

                ProgressConfig config = new ProgressConfig
                {
                    printLog = false,
                    progress = _fixedPercentage,
                };

                bool regexResult = MatchRegex(line, "(\\w+)\\.cpp", match =>
                {
                    config.info = $"Compiled {match}.cpp";
                    return true;
                });
                if (!regexResult)
                {
                    regexResult = MatchRegex(line, "(\\w+)\\.dll", match =>
                    {
                        config.info = $"Linked {match}.dll";
                        return true;
                    });
                }

                if (!regexResult) return;
                configs.Enqueue(config);
            }
        }
    }
}