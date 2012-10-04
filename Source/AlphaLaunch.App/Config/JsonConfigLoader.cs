using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AlphaLaunch.App.Config
{
    public class JsonConfigLoader
    {
        public JsonConfiguration Load(string file)
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullName = Path.Combine(directoryName, file);

            if (File.Exists(fullName))
            {
                return JsonConvert.DeserializeObject<JsonConfiguration>(File.ReadAllText(fullName));
            }

            return new JsonConfiguration();
        }

        public void Save(JsonConfiguration config, string file)
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullName = Path.Combine(directoryName, file);

            var json = JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            File.WriteAllText(fullName, json);
        }
    }
}