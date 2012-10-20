using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.Core.Config
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