using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AlphaLaunch.Core.Config
{
    public class JsonConfigLoader
    {
        public JsonUserConfiguration LoadUserConfig(string file)
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullName = Path.Combine(directoryName, file);

            if (File.Exists(fullName))
            {
                return JsonConvert.DeserializeObject<JsonUserConfiguration>(File.ReadAllText(fullName));
            }

            return new JsonUserConfiguration();
        }

        public void SaveUserConfig(JsonUserConfiguration config, string file)
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullName = Path.Combine(directoryName, file);

            var json = JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            File.WriteAllText(fullName, json);
        }

        public JsonIndexData LoadIndexData(string file)
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullName = Path.Combine(directoryName, file);

            if (File.Exists(fullName))
            {
                return JsonConvert.DeserializeObject<JsonIndexData>(File.ReadAllText(fullName));
            }

            return new JsonIndexData();
        }

        public void SaveIndexData(JsonIndexData data, string file)
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullName = Path.Combine(directoryName, file);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            File.WriteAllText(fullName, json);
        }
    }
}