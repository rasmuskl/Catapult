using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Catapult.Core.Config;

public class JsonConfigLoader
{
    public JsonUserConfiguration LoadUserConfig(string file)
    {
        var fileContents = ReadEntireFile(file);

        if (fileContents == null)
        {
            return JsonUserConfiguration.BuildDefaultSettings();
        }

        try
        {
            return JsonConvert.DeserializeObject<JsonUserConfiguration>(fileContents) ?? JsonUserConfiguration.BuildDefaultSettings();
        }
        catch (Exception e)
        {
            Log.Error(e, "Loading config failed.");
            throw;
        }
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
        File.WriteAllText(file, contents);
    }

    private string ReadEntireFile(string file)
    {
        if (!File.Exists(file))
        {
            return null;
        }

        return File.ReadAllText(file);
    }
}