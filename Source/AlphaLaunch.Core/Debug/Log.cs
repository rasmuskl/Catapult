using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.Core.Debug
{
    public static class Log
    {
        private static readonly List<string> Buffer = new List<string>();
        private static readonly List<Action<string>> Listeners = new List<Action<string>>();

        public static void Info(string line)
        {
            if (Listeners.Any())
            {
                foreach (var listener in Listeners)
                {
                    listener(line);
                }
            }
            else
            {
                Buffer.Add(line);
            }
        }

        public static void Attach(Action<string> listener)
        {
            foreach (var line in Buffer)
            {
                listener(line);
            }

            Listeners.Add(listener);
            Buffer.Clear();
        }
    }
}