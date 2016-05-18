using System.Drawing;

namespace Catapult.Core.Icons
{
    public class StaticIconResolver : IIconResolver
    {
        private readonly Icon _icon;

        public StaticIconResolver(Icon icon)
        {
            _icon = icon;
        }

        public Icon Resolve()
        {
            return _icon;
        }

        public string IconKey => null;
    }
}