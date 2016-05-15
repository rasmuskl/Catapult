using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using Serilog;

namespace Catapult.Core.Icons
{
    public class FaviconIconResolver : IIconResolver
    {
        private readonly string _iconUrl;

        public FaviconIconResolver(string iconUrl)
        {
            _iconUrl = iconUrl;
        }

        public Icon Resolve()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var bytes = httpClient.GetByteArrayAsync(_iconUrl).Result;

                    using (var stream = new MemoryStream(bytes))
                    {
                        return new Icon(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Loading favicon for {url} failed.", _iconUrl);
            }

            return null;
        }

        public string IconKey => $"Favicon: {_iconUrl}";
    }
}