using System.Diagnostics;
using System.Drawing;
using System.Net;
using Serilog;

namespace Catapult.Core.Icons;

public class UrlIconResolver : IIconResolver
{
    private readonly string _imageUrl;

    public UrlIconResolver(string imageUrl)
    {
        _imageUrl = imageUrl;
    }

    public Icon Resolve()
    {
        if (_imageUrl.IsNullOrWhiteSpace())
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
                    bytes = webClient.DownloadData(_imageUrl);
                }
                finally
                {
                    stopwatch.Stop();
                    Log.Information("Fetched url image in {time} - {url}", stopwatch.ElapsedMilliseconds, _imageUrl);
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
            Log.Error(ex, "Loading image for {url} failed.", _imageUrl);
        }

        return null;
    }

    public string IconKey => $"Url-image: {_imageUrl}";
}