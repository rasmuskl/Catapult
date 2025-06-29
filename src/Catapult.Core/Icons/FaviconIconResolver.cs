using System.Drawing;
using Serilog;

namespace Catapult.Core.Icons;

public class FaviconIconResolver(string iconUrl) : IIconResolver
{
    public Icon? Resolve()
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var bytes = httpClient.GetByteArrayAsync(iconUrl).Result;

                using (var stream = new MemoryStream(bytes))
                {
                    return new Icon(stream);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Loading favicon for {url} failed.", iconUrl);
        }

        return null;
    }

    public string IconKey => $"Favicon: {iconUrl}";
}