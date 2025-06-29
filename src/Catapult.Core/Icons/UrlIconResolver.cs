using System.Diagnostics;
using System.Drawing;
using System.Net;
using Serilog;

namespace Catapult.Core.Icons;

public class UrlIconResolver(string imageUrl) : IIconResolver
{
    public Icon? Resolve()
    {
        if (imageUrl.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            using (var webClient = new WebClient())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                byte[] bytes;
                try
                {
                    bytes = webClient.DownloadData(imageUrl);
                }
                finally
                {
                    stopwatch.Stop();
                    Log.Information("Fetched url image in {time} - {url}", stopwatch.ElapsedMilliseconds, imageUrl);
                }

                using (var stream = new MemoryStream(bytes))
                {
                    var bitmap = new Bitmap(stream);
                    return Icon.FromHandle(bitmap.GetHicon());
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Loading image for {url} failed.", imageUrl);
        }

        return null;
    }

    public string IconKey => $"Url-image: {imageUrl}";
}