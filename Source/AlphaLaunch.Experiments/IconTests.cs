using AlphaLaunch.Core.Icons;
using NUnit.Framework;

namespace AlphaLaunch.Experiments
{
    public class IconTests
    {

        [Test]
        public void Test()
        {
            ShellIcons.GetIcon(@"C:\Users\rasmuskl\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Spotify.lnk");
        }
    }
}