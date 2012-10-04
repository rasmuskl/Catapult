using System.Collections.Generic;

namespace AlphaLaunch.App.Config
{
    public class JsonConfiguration
    {
        public JsonConfiguration()
        {
            Paths = new string[0];
        }

        public string[] Paths { get; set; }
    }
}