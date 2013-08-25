using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.Core.Config
{
    public class JsonUserConfiguration
    {
        public JsonUserConfiguration()
        {
            Paths = new string[0];
        }

        public string[] Paths { get; set; }
    }
}