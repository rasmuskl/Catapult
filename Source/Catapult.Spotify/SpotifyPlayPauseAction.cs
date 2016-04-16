using Catapult.Core.Actions;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
{
    public class SpotifyPlayPauseAction : IStandaloneAction
    {
        public void RunAction()
        {
            new SpotifyClient().PlayPause();
        }

        public string Name
        {
            get { return "Spotify: Play / Pause"; }
        }

        public string BoostIdentifier
        {
            get { return typeof(SpotifyPlayPauseAction).Name; }
        }

        public object GetDetails()
        {
            return Name;
        }

        public IIconResolver GetIconResolver()
        {
            return new EmptyIconResolver();
        }
    }
}