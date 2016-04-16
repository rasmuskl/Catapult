using System.Deployment.Application;
using System.IO;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Catapult.Core.Config
{
    public class JsonConfigLoader
    {
        public JsonUserConfiguration LoadUserConfig(string file)
        {
            var fileContents = ReadEntireFile(file);

            if (fileContents == null)
            {
                return new JsonUserConfiguration();
            }

            return JsonConvert.DeserializeObject<JsonUserConfiguration>(fileContents);
        }

        public void SaveUserConfig(JsonUserConfiguration config, string file)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            WriteEntireFile(file, json);
        }

        public JsonIndexData LoadIndexData(string file)
        {
            var fileContents = ReadEntireFile(file);

            if (fileContents == null)
            {
                return new JsonIndexData();
            }

            return JsonConvert.DeserializeObject<JsonIndexData>(fileContents);
        }

        public void SaveIndexData(JsonIndexData data, string file)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            WriteEntireFile(file, json);
        }

        private void WriteEntireFile(string file, string contents)
        {
            var scope = GetIsolatedStorageFileScope();

            using (var stream = scope.OpenFile(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(contents);
                }
            }
        }

        private string ReadEntireFile(string file)
        {
            var scope = GetIsolatedStorageFileScope();

            if (!scope.FileExists(file))
            {
                return null;
            }
            
            using (var stream = scope.OpenFile(file, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static IsolatedStorageFile GetIsolatedStorageFileScope()
        {
            return ApplicationDeployment.IsNetworkDeployed
                ? IsolatedStorageFile.GetUserStoreForApplication()
                : IsolatedStorageFile.GetUserStoreForDomain();
        }
    }
}