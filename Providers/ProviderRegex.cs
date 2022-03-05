using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ClipboardDetector.Providers
{
    public class ProviderRegex
    {
        enum State
        {
            WaitKey,
            WaitValue,
        }
        public static void Load(string path, Dictionary<string, string> map)
        {
            map.Clear();
            string key = null;
            var state = State.WaitKey;
            var valueLines = new List<string>();
            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                if (line.StartsWith('#'))
                    continue;
                var isEmpty = string.IsNullOrEmpty(line);

                switch (state)
                {
                    case State.WaitKey:
                        if (isEmpty) break;
                        key = line;
                        state = State.WaitValue;
                        break;

                    case State.WaitValue:
                        if (isEmpty)
                        {
                            var value = string.Join("\n", valueLines);
                            map.Add(key, value);
                            state = State.WaitKey;
                        }
                        else
                            valueLines.Add(line);
                        break;
                }
            }
            if (valueLines.Count > 0)
                map.Add(key, string.Join("\n", valueLines));
        }
    }
}
